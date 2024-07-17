using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.Scripting.Action;
using Long.Kernel.States.Items;
using Long.Kernel.States.Status;
using Long.Kernel.States.World;
using Long.Network.Packets;
using Long.Shared.Mathematics;
using Long.World.Map;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;
using static Long.Kernel.States.Items.ItemBase;

namespace Long.Kernel.States.User
{
    public partial class Character : Role
    {
        private static readonly ILogger logger = Log.ForContext<Character>();
        private readonly DbUser user;

        private readonly TimeOut tickTimer = new();
        private readonly TimeOut dateSyncTimer = new();

        private readonly TimeOutMS slowHealLife = new(SLOWHEALLIFE_MS);
        private readonly TimeOutMS slowHealMana = new(SLOWHEALLIFE_MS);
        private readonly List<int> setSlowHealUp2Life = new();
        private readonly List<int> setSlowHealUp2Mana = new();

        public Character(GameClientBase client, DbUser user)
        {
            this.user = user;
            if (client == null)
            {
                return;
            }

            Client = client;

            mesh = user.Lookface;
            currentX = user.X;
            currentY = user.Y;
            idMap = user.MapId;

            Screen = new Screen(this);
            Packages = new Package(this);
            UserPackage = new UserPackage(this);
            StatusSet = new StatusSet(this);
            Statistic = new UserStatistic(this);
            Action = EntityAction.Stand;

            energyTimer.Update();
            pkDecreaseTimer.Update();
            xpPointsTimer.Update();
            dateSyncTimer.Startup(30);
            tickTimer.Startup(10);
            slowHealLife.Update();
            slowHealMana.Update();
        }

        public GameClientBase Client { get; }

        public Screen Screen { get; }

        #region Identity

        public override uint Identity
        {
            get => user.Identity;
        }

        public override string Name
        {
            get => user.Name;
        }

        public void ChangeName(string newName)
        {
            user.Name = newName;
        }

        public string MateName
        {
            get => user.Mate;
            set => user.Mate = value;
        }

        public uint MateIdentity
        {
            get;
            set;
        }

        #endregion

        #region Authority

        public bool IsPm()
        {
            return Name.Contains("[PM]");
        }

        public bool IsGm()
        {
            return IsPm() || Name.Contains("[GM]");
        }

        #endregion

        #region Appearence

        private uint mesh;
        private ushort transformationMesh;

        public int Gender => Body == BodyType.Male ? 1 : 2;

        public ushort TransformationMesh
        {
            get => transformationMesh;
            set
            {
                transformationMesh = value;
                Mesh = (uint)((uint)value * 10000000 + Avatar * 10000 + (uint)Body);
            }
        }

        public override uint Mesh
        {
            get => mesh;
            set
            {
                mesh = value;
                user.Lookface = value % 10000000;
            }
        }

        public BodyType Body
        {
            get => (BodyType)(Mesh % 10000);
            set => Mesh = (uint)value + Avatar * 10000u;
        }

        public ushort Avatar
        {
            get => (ushort)(Mesh % 10000000 / 10000);
            set => Mesh = (uint)(value * 10000 + (Mesh % 10000));
        }

        public ushort Hairstyle
        {
            get => user.Hairstyle;
            set => user.Hairstyle = value;
        }

        #endregion

        #region Profession

        public byte ProfessionSort => (byte)(Profession / 10);

        public byte ProfessionLevel => (byte)(Profession % 10);

        public override int Profession
        {
            get => user?.Profession ?? 0;
            set => user.Profession = (byte)value;
        }

        public byte PreviousProfession
        {
            get;
            set;
        }

        public byte FirstProfession
        {
            get;
            set;
        }

        #endregion

        #region Experience

        public bool AutoAllot
        {
            get => user.AutoAllot != 0;
            set => user.AutoAllot = (byte)(value ? 1 : 0);
        }

        public override byte Level
        {
            get => user?.Level ?? 0;
            set => user.Level = Math.Min(MAX_UPLEV, Math.Max((byte)1, value));
        }

        public ulong Experience
        {
            get => user?.Experience ?? 0;
            set
            {
                if (Level >= MAX_UPLEV)
                {
                    return;
                }

                user.Experience = value;
            }
        }

        public byte Metempsychosis
        {
            get => user?.Metempsychosis ?? 0;
            set => user.Metempsychosis = value;
        }

        #endregion

        #region Attribute Points

        public ushort Strength
        {
            get => user?.Power ?? 0;
            set => user.Power = value;
        }

        public ushort Speed
        {
            get => user?.Agility ?? 0;
            set => user.Agility = value;
        }

        public ushort Vitality
        {
            get => user?.Vitality ?? 0;
            set => user.Vitality = value;
        }

        public ushort Spirit
        {
            get => user?.Spirit ?? 0;
            set => user.Spirit = value;
        }

        public ushort AttributePoints
        {
            get => user?.AttributePoints ?? 0;
            set => user.AttributePoints = value;
        }

        public int TotalAttributePoints => Strength + Speed + Vitality + Spirit + AttributePoints;

        public byte AdjustSpeed(int nSpeed)
        {
            if (StatusSet == null)
            {
                return (byte)nSpeed;
            }

            int nAddSpeed = 0;
            IStatus pStatus = QueryStatus(StatusSet.STATUS_TEAMSPEED);
            if (pStatus != null)
            {
                nAddSpeed += nSpeed * pStatus.Power / 100;
            }

            pStatus = QueryStatus(StatusSet.STATUS_PELT);
            if (pStatus != null)
            {
                nAddSpeed += Calculations.CutTrail(0, Calculations.AdjustData(nSpeed, pStatus.Power)) - nSpeed;
            }

            pStatus = QueryStatus(StatusSet.STATUS_SLOWDOWN1);
            if (pStatus != null)
            {
                nAddSpeed += Calculations.CutTrail(0, Calculations.AdjustData(nSpeed, pStatus.Power)) - nSpeed;
            }

            pStatus = QueryStatus(StatusSet.STATUS_SLOWDOWN2);
            if (pStatus != null && (Life * 2 < MaxLife))
            {
                nAddSpeed += Calculations.CutTrail(0, Calculations.AdjustData(nSpeed, pStatus.Power)) - nSpeed;
            }

            nSpeed = Math.Min(255, Math.Max(1, nSpeed + nAddSpeed));
            return (byte)nSpeed;
        }

        #endregion

        #region Life and Mana

        public override uint Life
        {
            get => user.HealthPoints;
            set => user.HealthPoints = Math.Min(MaxLife, value);
        }

        public override uint MaxLife
        {
            get
            {
                var result = (uint)(Vitality * 10);
                for (ItemPosition pos = ItemPosition.EquipmentBegin; pos <= ItemPosition.EquipmentEnd; pos++)
                {
                    result += (uint)(UserPackage.GetEquipment(pos)?.Life ?? 0);
                }

                return result;
            }
        }

        public int MaxLifePercent
        {
            get => user.MaxLifePercent;
            set => user.MaxLifePercent = value;
        }

        public override uint Mana
        {
            get => user.ManaPoints;
            set => user.ManaPoints = (ushort)Math.Min(MaxMana, value);
        }

        public override uint MaxMana
        {
            get
            {
                var result = (uint)(Spirit * 20);
                for (ItemPosition pos = ItemPosition.EquipmentBegin; pos <= ItemPosition.EquipmentEnd; pos++)
                {
                    result += (uint)(UserPackage.GetEquipment(pos)?.Mana ?? 0);
                }
                return result;
            }
        }

        public async Task AddSlowRealLifeAsync(int nAddLife, int nAddMana, int nTimes)
        {
            if (nAddLife > 0)
            {
                int nFirstAdd = (nTimes < 2) ? nAddLife : 2 * nAddLife / (nTimes + 1);
                int nNextAdd = (nTimes >= 2) ? (nAddLife - nFirstAdd) / (nTimes - 1) : 0;
                int nLastAdd = nAddLife - nFirstAdd;
                if (nTimes > 2)
                {
                    nLastAdd -= nNextAdd * (nTimes - 2);
                }

                List<int> setAddLife = new();
                if (setSlowHealUp2Life.Count == 0)
                {
                    await AddAttributesAsync(ClientUpdateType.Life, nFirstAdd);
                    //await BroadcastTeamLifeAsync();
                }
                else
                {
                    setAddLife.Add(nFirstAdd);
                }

                if (nNextAdd > 0)
                {
                    for (int i = 1; i < nTimes - 1; i++)
                    {
                        setAddLife.Add(nNextAdd);
                    }
                }

                if (nLastAdd > 0)
                {
                    setAddLife.Add(nFirstAdd);
                }

                int nMinNum = Math.Min(setAddLife.Count, setSlowHealUp2Life.Count);
                for (int i = 0; i < nMinNum; i++)
                {
                    if (setAddLife[i] > setSlowHealUp2Life[i])
                    {
                        setSlowHealUp2Life[i] = setAddLife[i];
                    }
                }

                for (int i = nMinNum; i < setAddLife.Count; i++)
                {
                    setSlowHealUp2Life.Add(setAddLife[i]);
                }

                slowHealLife.Update();
            }
            else if (nAddLife < 0)
            {
                await AddAttributesAsync(ClientUpdateType.Life, nAddLife);
                //await BroadcastTeamLifeAsync();
            }

            if (nAddMana > 0)
            {
                int nFirstAdd = (nTimes < 2) ? nAddMana : 2 * nAddMana / (nTimes + 1);
                int nNextAdd = (nTimes >= 2) ? (nAddMana - nFirstAdd) / (nTimes - 1) : 0;
                int nLastAdd = nAddMana - nFirstAdd;
                if (nTimes > 2)
                {
                    nLastAdd -= nNextAdd * (nTimes - 2);
                }

                List<int> setAddMana = new();
                if (setSlowHealUp2Mana.Count == 0)
                {
                    await AddAttributesAsync(ClientUpdateType.Mana, nFirstAdd);
                }
                else
                {
                    setAddMana.Add(nFirstAdd);
                }

                if (nNextAdd > 0)
                {
                    for (int i = 1; i < nTimes - 1; i++)
                    {
                        setAddMana.Add(nNextAdd);
                    }
                }

                if (nLastAdd > 0)
                {
                    setAddMana.Add(nFirstAdd);
                }

                int nMinNum = Math.Min(setAddMana.Count, setSlowHealUp2Mana.Count);
                for (int i = 0; i < nMinNum; i++)
                {
                    if (setAddMana[i] > setSlowHealUp2Mana[i])
                    {
                        setSlowHealUp2Mana[i] = setAddMana[i];
                    }
                }

                for (int i = nMinNum; i < setAddMana.Count; i++)
                {
                    setSlowHealUp2Mana.Add(setAddMana[i]);
                }

                slowHealMana.Update();
            }
            else if (nAddMana < 0)
            {
                await AddAttributesAsync(ClientUpdateType.Mana, nAddMana);
            }

            //// cheat check
            //{
            //    if ((IsCheater(_TYPE_WS) || IsCheater(_TYPE_FY)) && IsCheater(_TYPE_USE_LIFE) && ::RandGet(3) == 0)
            //    {
            //        KickoutCheat(_TYPE_USE_LIFE);
            //    }
            //}
        }

        public async Task ProcSlowHealLifeUpAsync()
        {
            if (setSlowHealUp2Life.Count == 0)
            {
                return;
            }

            if (!IsAlive || Life >= MaxLife)
            {
                setSlowHealUp2Life.Clear();
                return;
            }

            int nAddLife = setSlowHealUp2Life[0];
            setSlowHealUp2Life.RemoveAt(0);
            await AddAttributesAsync(ClientUpdateType.Life, nAddLife);
            //await BroadcastTeamLifeAsync();
        }

        public async Task ProcSlowHealManaUpAsync()
        {
            if (setSlowHealUp2Mana.Count == 0)
            {
                return;
            }

            if (!IsAlive || Mana >= MaxMana)
            {
                setSlowHealUp2Mana.Clear();
                return;
            }

            int nAddMana = setSlowHealUp2Mana[0];
            setSlowHealUp2Mana.RemoveAt(0);
            await AddAttributesAsync(ClientUpdateType.Mana, nAddMana);
        }

        #endregion

        #region Currency

        public ulong Silvers
        {
            get => user?.Money ?? 0;
            set => user.Money = value;
        }

        public uint EudemonPoints
        {
            get => user?.Emoney ?? 0;
            set => user.Emoney = value;
        }

        public uint EudemonPointsBound
        {
            get => user?.EmoneyMono ?? 0;
            set => user.EmoneyMono = value;
        }

        public uint StorageMoney
        {
            get => user?.MoneySaved ?? 0;
            set => user.MoneySaved = value;
        }

        public uint StudyPoints
        {
            get;
            set;
        }

        public uint ChiPoints
        {
            get;
            set;
        }

        public uint HorseRacingPoints
        {
            get;
            set;
        }

        public async Task<bool> ChangeMoneyAsync(long amount, bool notify = false)
        {
            if (amount > 0)
            {
                await AwardMoneyAsync(amount);
                return true;
            }
            else if (amount < 0)
            {
                return await SpendMoneyAsync(amount * -1, notify);
            }
            return false;
        }

        public async Task AwardMoneyAsync(long amount)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid award money amount {0} for user {1} {2}", amount, Identity, Name);
                return;
            }

            Silvers = Math.Min(MAX_INVENTORY_MONEY, Silvers + (ulong)amount);
            await SynchroAttributesAsync(ClientUpdateType.Money, Silvers);
            await SaveAsync();
        }

        public async Task<bool> SpendMoneyAsync(long amount, bool notify = false)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid spend money amount {0} for user {1} {2}", amount, Identity, Name);
                return false;
            }

            if ((ulong)amount > Silvers)
            {
                if (notify)
                {
                    await SendAsync(StrNotEnoughMoney);
                }
                return false;
            }

            Silvers = (ulong)Math.Max(0, (long)Silvers - amount);
            await SynchroAttributesAsync(ClientUpdateType.Money, Silvers);
            await SaveAsync();
            return true;
        }

        public async Task<bool> ChangeEudemonPointsAsync(int amount, bool notify = false)
        {
            if (amount == 0)
            {
                return false;
            }
            if (amount > 0)
            {
                await AwardEudemonPointsAsync(amount);
                return true;
            }
            return await SpendEudemonPointsAsync(amount * -1, notify);
        }

        public async Task AwardEudemonPointsAsync(int amount)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid award emoney amount {0} for user {1} {2}", amount, Identity, Name);
                return;
            }

            EudemonPoints = (uint)Math.Min(MAX_INVENTORY_EMONEY, EudemonPoints + amount);
            await SynchroAttributesAsync(ClientUpdateType.EudemonPoints, EudemonPoints);
        }

        public async Task<bool> SpendEudemonPointsAsync(int amount, bool notify = false)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid spend emoney amount {0} for user {1} {2}", amount, Identity, Name);
                return false;
            }

            if (amount > EudemonPoints)
            {
                if (notify)
                {
                    await SendAsync(StrNotEnoughEmoney);
                }
                return false;
            }

            EudemonPoints = (uint)Math.Max(0, EudemonPoints - amount);
            await SynchroAttributesAsync(ClientUpdateType.EudemonPoints, EudemonPoints);
            return true;
        }

        public async Task SaveEmoneyLogAsync(EmoneyOperationType type, uint target, uint targetBalance, long amount)
        {
            if (amount == 0)
            {
                return;
            }

            if (amount < 0)
            {
                amount *= -1;
            }

            uint timestamp = (uint)UnixTimestamp.Now;
            uint checkSum = CalculateEmoneyCheckSum(EudemonPoints, Identity);
            user.EmoneyChkSum = checkSum;
            using var context = new ServerDbContext();
            context.Users.Update(user);
            context.EMoneyLogs.Add(new DbEMoney
            {
                IdSource = Identity,
                IdTarget = target,
                Number = (uint)amount,
                ChkSum = checkSum,
                TimeStamp = timestamp,
                Type = (byte)type,
                TargetBalance = targetBalance,
                SourceBalance = EudemonPoints
            });
            await context.SaveChangesAsync();
        }

        public async Task<bool> ChangeBoundEudemonPointsAsync(int amount, bool notify = false)
        {
            if (amount == 0)
            {
                return false;
            }
            if (amount > 0)
            {
                await AwardBoundEudemonPointsAsync(amount);
                return true;
            }
            return await SpendBoundEudemonPointsAsync(amount * -1, notify);
        }

        public async Task AwardBoundEudemonPointsAsync(int amount)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid award emoney(B) amount {0} for user {1} {2}", amount, Identity, Name);
                return;
            }

            EudemonPointsBound = (uint)Math.Min(MAX_INVENTORY_EMONEY, EudemonPointsBound + amount);
            await SynchroAttributesAsync(ClientUpdateType.EudemonPointsBound, EudemonPointsBound);
        }

        public async Task<bool> SpendBoundEudemonPointsAsync(int amount, bool notify = false)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid spend emoney(B) amount {0} for user {1} {2}", amount, Identity, Name);
                return false;
            }

            if (amount > EudemonPointsBound)
            {
                if (notify)
                {
                    await SendAsync(StrNotEnoughEmoneyMono);
                }
                return false;
            }

            EudemonPointsBound = (uint)Math.Max(0, EudemonPointsBound - amount);
            await SynchroAttributesAsync(ClientUpdateType.EudemonPointsBound, EudemonPointsBound);
            return true;
        }

        public async Task<bool> SpendBoundEudemonPointsAsync(EmoneyOperationType type, int amount, bool notify = false)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid spend emoney(B) amount {0} for user {1} {2}", amount, Identity, Name);
                return false;
            }

            int totalConquerPoints = (int)(EudemonPoints + EudemonPointsBound);
            if (amount > EudemonPointsBound)
            {
                if (amount > totalConquerPoints)
                {
                    if (notify)
                    {
                        await SendAsync(StrNotEnoughEmoneyMono);
                    }
                    return false;
                }

                int spendFromEmoneyMono = (int)EudemonPointsBound;
                int spendFromEmoney = amount - (int)EudemonPointsBound;
                await SpendBoundEudemonPointsAsync((int)EudemonPointsBound);
                await SpendEudemonPointsAsync(spendFromEmoney);

                await SaveEmoneyMonoLogAsync(type, 0, 0, (uint)spendFromEmoneyMono);
                await SaveEmoneyLogAsync(type, 0, 0, (uint)spendFromEmoney);
                return true;
            }

            if (!await SpendBoundEudemonPointsAsync(amount, notify))
            {
                return false;
            }

            await SaveEmoneyMonoLogAsync(type, 0, 0, (uint)amount);
            return true;
        }

        public async Task SaveEmoneyMonoLogAsync(EmoneyOperationType type, uint target, uint targetBalance, long amount)
        {
            if (amount == 0)
            {
                return;
            }

            if (amount < 0)
            {
                amount *= -1;
            }

            uint timestamp = (uint)UnixTimestamp.Now;
            uint checkSum = CalculateEmoneyCheckSum(EudemonPointsBound, Identity);
            using var context = new ServerDbContext();
            context.Users.Update(user);
            context.EMoneyMonoLogs.Add(new DbEMoneyMono
            {
                IdSource = Identity,
                IdTarget = target,
                Number = (uint)amount,
                ChkSum = checkSum,
                TimeStamp = timestamp,
                Type = (byte)type,
                TargetBalance = targetBalance,
                SourceBalance = EudemonPointsBound
            });
            await context.SaveChangesAsync();
        }

        public static uint CalculateEmoneyCheckSum(uint value, uint identity)
        {
            value ^= identity;
            value = (value + 0x7ed55d16) + (value << 12);
            value = (value ^ 0xc761c23c) ^ (value >> 19);
            value = (value + 0x165667b1) + (value << 5);
            value = (value + 0xd3a2646c) ^ (value << 9);
            value = (value + 0xfd7046c5) + (value << 3);
            value = (value ^ 0xb55a4f09) ^ (value >> 16);
            return value;
        }



        #endregion

        #region Wood

        public uint Wood
        {
            get => user.Wood;
            set => user.Wood = value;
        }

        public async Task<bool> ChangeWoodAsync(int amount, bool notify = false)
        {
            if (amount > 0)
            {
                await AwardWoodAsync(amount);
                return true;
            }
            else if (amount < 0)
            {
                return await SpendWoodAsync(amount * -1, notify);
            }
            return false;
        }

        public async Task AwardWoodAsync(int amount)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid award wood amount {0} for user {1} {2}", amount, Identity, Name);
                return;
            }

            Wood = Math.Min(3999999000, Wood + (uint)amount);
            await SynchroAttributesAsync(ClientUpdateType.Wood, Wood);
            await SaveAsync();
        }

        public async Task<bool> SpendWoodAsync(int amount, bool notify = false)
        {
            if (amount <= 0)
            {
                logger.Warning("Invalid spend wood amount {0} for user {1} {2}", amount, Identity, Name);
                return false;
            }

            if ((uint)amount > Wood)
            {
                if (notify)
                {
                    await SendAsync("Você não tem madeira suficiente!");
                }
                return false;
            }

            Wood = (uint)Math.Max(0, (long)Wood - amount);
            await SynchroAttributesAsync(ClientUpdateType.Wood, Wood);
            await SaveAsync();
            return true;
        }

        #endregion

        #region Mercenary

        public int MercenaryRank
        {
            get => user.MercenaryRank;
            set => user.MercenaryRank = value;
        }

        public int MercenaryExp
        {
            get => user.MercenaryExp;
            set => user.MercenaryExp = value;
        }

        #endregion

        #region Exploit

        public int Exploit
        {
            get => user.Exploit;
            set => user.Exploit = value;
        }

        public uint BonusPoint
        {
            get => user.BonusPoints;
            set => user.BonusPoints = value;
        }


        #endregion

        #region Tutoring

        public byte TutorLevel
        {
            get => user.TutorLevel;
            set => user.TutorLevel = value;
        }

        public uint TutorExp
        {
            get => user.TutorExp;
            set => user.TutorExp = value;
        }

        #endregion

        #region PK

        private readonly TimeOut pkDecreaseTimer = new(PK_DEC_TIME);

        public PkModeType PkMode { get; set; }

        public ushort PkPoints
        {
            get => user?.KillPoints ?? 0;
            set => user.KillPoints = value;
        }

        public async Task SetPkModeAsync(PkModeType mode)
        {
            PkMode = mode;
            await SendAsync(new MsgAction
            {
                Identity = Identity,
                Action = EOActionType.actionSetPkMode,
                Data = (uint)PkMode
            });
        }

        #endregion

        #region Position

        /// <summary>
        ///     The current map identity for the role.
        /// </summary>
        public override uint MapIdentity
        {
            get => idMap;
            set => idMap = value;
        }

        /// <summary>
        ///     Current X position of the user in the map.
        /// </summary>
        public override ushort X
        {
            get => currentX;
            set => currentX = value;
        }

        /// <summary>
        ///     Current X position of the user in the map.
        /// </summary>
        public override ushort Y
        {
            get => currentY;
            set => currentY = value;
        }

        public uint RecordMapIdentity
        {
            get => user.MapId;
            set => user.MapId = value;
        }

        public ushort RecordMapX
        {
            get => user.X;
            set => user.X = value;
        }

        public ushort RecordMapY
        {
            get => user.Y;
            set => user.Y = value;
        }

        public uint GodRecMapId { get; set; }
        public ushort GodRecMapX { get; set; }
        public ushort GodRecMapY { get; set; }

        public override async Task EnterMapAsync()
        {
            Map = MapManager.GetMap(idMap);
            if (Map != null)
            {
                await Map.AddAsync(this);
                await Map.SendMapInfoAsync(this);
                await ProcessAfterMoveAsync();
                await OnEnterMapAsync(this, Map);
            }
        }

        public override async Task LeaveMapAsync()
        {
            if (Map != null)
            {
                await ProcessOnMoveAsync(MsgWalkEx.RoleMoveMode.Walk);
                await OnLeaveMapAsync(this, Map);
                await Map.RemoveAsync(Identity);
            }

            await Screen.ClearAsync();
        }

        public async Task SavePositionAsync(uint idMap, ushort x, ushort y)
        {
            GameMap map = MapManager.GetMap(idMap);
            if (map?.IsRecordDisable() == false)
            {
                user.X = x;
                user.Y = y;
                user.MapId = idMap;
                await SaveAsync();
            }
        }

        public async Task<bool> FlyMapAsync(uint idMap, int x, int y)
        {
            if (Map == null)
            {
                logger.Warning("FlyMap user [{Identity}] not in map", Identity);
                return false;
            }

            if (idMap == 0)
            {
                idMap = MapIdentity;
            }

            GameMap newMap = MapManager.GetMap(idMap);
            if (newMap == null || !newMap.IsValidPoint(x, y))
            {
                logger.Error("FlyMap user fly invalid position {idMap}[{x},{y}]", idMap, x, y);
                return false;
            }

            if (!newMap.IsStandEnable(x, y))
            {
                bool succ = false;
                for (int i = 0; i < 8; i++)
                {
                    int testX = x + GameMapData.WalkXCoords[i];
                    int testY = y + GameMapData.WalkYCoords[i];

                    if (newMap.IsStandEnable(testX, testY))
                    {
                        x = testX;
                        y = testY;
                        succ = true;
                        break;
                    }
                }

                if (!succ)
                {
                    newMap = MapManager.GetMap(1000);
                    x = 178;
                    y = 416;
                }
            }

            try
            {
                await LeaveMapAsync(); // leave map on current partition

                this.idMap = newMap.Identity;
                X = (ushort)x;
                Y = (ushort)y;

                async Task characterEnterMapTask()
                {
                    await EnterMapAsync();
                    await SendAsync(new MsgAction
                    {
                        Identity = Identity,
                        CommandX = X,
                        CommandY = Y,
                        Action = EOActionType.actionFlyMap,
                        Argument = (ushort)Direction,
                        Data = newMap.MapDoc,
                    });
                    //await Screen.UpdateAsync();
                    await Screen.SynchroScreenAsync();
                }

                if (newMap.Partition == Map.Partition)
                {
                    await characterEnterMapTask();
                }
                else
                {
                    QueueAction(characterEnterMapTask);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Fly map error", ex.Message);
            }
            return true;
        }

        public async Task<bool> SynPositionAsync(ushort x, ushort y, int nMaxDislocation)
        {
            if (nMaxDislocation <= 0 || x == 0 && y == 0) // ignore in this condition
            {
                return true;
            }

            int nDislocation = GetDistance(x, y);
            if (nDislocation >= nMaxDislocation)
            {
                return false;
            }

            if (nDislocation <= 0)
            {
                return true;
            }

            if (IsGm())
            {
                await SendAsync($"syn move: ({X},{Y})->({x},{y})", TalkChannel.Talk, Color.Red);
            }

            if (!Map.IsValidPoint(x, y))
            {
                return false;
            }

            await ProcessOnMoveAsync(MsgWalkEx.RoleMoveMode.Walk);
            await JumpPosAsync(x, y);
            await Screen.BroadcastRoomMsgAsync(new MsgAction
            {
                Identity = Identity,
                Action = EOActionType.actionKickBack,
                ArgumentX = x,
                ArgumentY = y,
                Command = (uint)((y << 16) | x),
                Direction = (ushort)Direction
            });

            return true;
        }

        public Task KickbackAsync()
        {
            return SendAsync(new MsgAction
            {
                Identity = Identity,
                Direction = (ushort)Direction,
                Data = MapIdentity,
                CommandX = X,
                CommandY = Y,
                Action = EOActionType.actionKickBack,
                Timestamp = (uint)Environment.TickCount
            });
        }

        public override async Task ProcessOnMoveAsync(MsgWalkEx.RoleMoveMode mode)
        {
            protectionTimer.Clear();
            await base.ProcessOnMoveAsync(mode);
        }

        public override async Task ProcessAfterMoveAsync()
        {
            energyTimer.Startup(ADD_ENERGY_STAND_MS);
            await base.ProcessAfterMoveAsync();
        }

        public override async Task ProcessOnAttackAsync()
        {
            energyTimer.Startup(ADD_ENERGY_STAND_MS);
            protectionTimer.Clear();
            await base.ProcessOnAttackAsync();
        }

        #endregion

        #region XP and Stamina

        private readonly TimeOutMS energyTimer = new(ADD_ENERGY_STAND_MS);
        private readonly TimeOut xpPointsTimer = new(XPTIME_TICK);
        private readonly TimeOut clearXpTimer = new();

        public int KoCount { get; set; }
        public override byte Energy { get; set; } = DEFAULT_USER_ENERGY;
        public byte MaxEnergy =>  100;
        public byte XpPoints { get; set; }

        public async Task ProcXpValAsync()
        {
            if (!IsAlive)
            {
                await ClsXpValAsync();
                return;
            }

            IStatus pStatus = QueryStatus(StatusSet.STATUS_XPFULL);
            if (pStatus != null)
            {
                return;
            }

            if (XpPoints >= 100)
            {
                await BurstXpAsync();
                xpPointsTimer.Update();
                clearXpTimer.Startup(XPTIME_KEEPEFFECT);
            }
            else
            {
                if (Map != null && Map.IsBoothEnable())
                {
                    return;
                }

                await AddXpAsync(XPTIME_INCAMOUNT);
            }
        }

        public async Task<bool> BurstXpAsync()
        {
            if (XpPoints < XPTIME_MAXVALUE)
            {
                return false;
            }

            IStatus pStatus = QueryStatus(StatusSet.STATUS_XPFULL);
            if (pStatus != null)
            {
                return true;
            }

            await AttachStatusAsync(this, StatusSet.STATUS_XPFULL, 0, XPTIME_KEEPEFFECT, 0);
            await BroadcastRoomMsgAsync(new MsgAction()
            {
                Action = EOActionType.actionSoundEffect,
                Identity = user.Identity,
                CommandX = X,
                CommandY = Y,
                Argument = (byte)Direction,
                Data = 1000000
            }, true);

            return true;
        }

        public async Task SetXpAsync(byte nXp)
        {
            if (nXp > XPTIME_MAXVALUE)
            {
                return;
            }

            await SetAttributesAsync(ClientUpdateType.Xp, nXp);
        }

        public async Task AddXpAsync(byte nXp)
        {
            if (nXp <= 0 || !IsAlive)
            {
                return;
            }

            await AddAttributesAsync(ClientUpdateType.Xp, nXp);
        }

        public async Task ClsXpValAsync()
        {
            await SetXpAsync(0);
            await StatusSet.DelObjAsync(StatusSet.STATUS_XPFULL);
        }

        public async Task FinishXpAsync()
        {
            //int currentPoints = SupermanManager.GetSupermanPoints(Identity);
            //if (KoCount >= 25 && currentPoints < KoCount)
            //{
            //    await SupermanManager.AddOrUpdateSupermanAsync(Identity, KoCount);
            //    int rank = SupermanManager.GetSupermanRank(Identity);
            //    if (rank < 100)
            //    {
            //        await RoleManager.BroadcastWorldMsgAsync(string.Format(StrSupermanBroadcast, Name, KoCount, rank), TalkChannel.Talk, Color.White);
            //    }
            //}
            KoCount = 0;
        }

        #endregion

        #region VIP

        public uint VipLevel { get; set; }

        #endregion

        #region Celebrity Hall
        public ushort HallOfFameRank { get; set; }

        // TODO: loading and etc...

        #endregion

        #region Business

        //private readonly TimeOut merchantTimerChk = new();

        public uint Merchant => user.Business;

        public bool IsMerchant => user.Business == 255;

        public bool IsMerchantReady => user.Business != 255 && user.Business >= uint.Parse(DateTime.Now.ToString("yyMMdd"));

        public async Task RemoveMerchantAsync()
        {
            user.Business = 0;
            await SaveAsync();
            await SendMenuMessageAsync("Seu status de Mercador foi removido com sucesso!");
        }

        public async Task SetMerchantAsync()
        {
            user.Business = uint.Parse(DateTime.Now.AddDays(3).ToString("yyMMdd"));
            await SendMenuMessageAsync("Você só precisa aguardar 3 dias para se tornar um Mercador, boa sorte!");
        }

        public async Task LoadMerchantAsync()
        {
            if (user.Business == 0)
            {
                return;
            }

            if (user.Business == 255)
            {
                await SendMenuMessageAsync("Você é um Mercador, cuidado para não se envolver em vendas suspeitas!");
                return;
            }

            if (user.Business != 0 && !IsMerchantReady)
            {
                int totalDays = (int)(DateTime.Now - new DateTime(((int)user.Business / 10000 + 2000), ((int)(user.Business % 10000) / 100), (int)user.Business % 100)).TotalDays;
                await SendMenuMessageAsync($"Você se tornará um Mercador em {totalDays} dias!");
            }
            else if (user.Business != 0 && IsMerchantReady)
            {
                user.Business = 255;
                await SaveAsync();
                await SendMenuMessageAsync("Seu status de Mercador habilitado com sucesso!");
            }
        }

        #endregion

        #region GameCard

        public int CountCard => 0;
        public int CountCard2 => 0;

        #endregion

        #region Leave word

        private Dictionary<string, string> awaitingLeaveWords = new();

        public async Task LoadLeaveWordAsync()
        {
            List<DbLeaveword> leavewords = LeavewordRepository.Get(Name);
            foreach (var leaveword in leavewords)
            {
                await SendAsync(new MsgTalk(TalkChannel.Offline, Color.FromArgb(255, 255, 255, 255), leaveword.Words)
                {
                    RecipientName = Name,
                    SenderMesh = leaveword.Lookface,
                    SenderName = leaveword.SendName,
                    Suffix = leaveword.Time
                });
            }
            await ServerDbContext.DeleteRangeAsync(leavewords);

            List<DbSysLeaveword> sysLeavewords = LeavewordRepository.GetSys(Identity);
            foreach (var leaveword in sysLeavewords)
            {
                await SendAsync(new MsgTalk(TalkChannel.Offline, Color.FromArgb(255, 255, 255, 255), leaveword.Words)
                {
                    RecipientName = Name,
                    SenderMesh = MsgTalk.SystemLookface,
                    SenderName = leaveword.SendName,
                    Suffix = UnixTimestamp.ToDateTime(leaveword.Time).ToString("yyyyMMddHHmmss")
                });
            }
            await ServerDbContext.DeleteRangeAsync(sysLeavewords);
        }

        public string GetPendingLeaveWord(string targetName)
        {
            return awaitingLeaveWords.TryGetValue(targetName, out var result) ? result : string.Empty;
        }

        public async Task ClearLeaveWordAsync(string targetName)
        {
            DbUser targetUser = await UserRepository.FindAsync(targetName);
            if (targetUser == null)
            {
                return;
            }

            List<DbLeaveword> leavewords = LeavewordRepository.GetWords(Name, targetUser.Name)
                .Where(x => x.UserName == targetUser.Name)
                .ToList();

            if (leavewords.Count > 0)
            {
                await ServerDbContext.DeleteRangeAsync(leavewords);
            }
        }

        public async Task LeaveWordAsync(string targetName, string message, bool skip = false)
        {
            DbUser targetUser = await UserRepository.FindAsync(targetName);
            if (targetUser == null)
            {
                return;
            }

            var words = LeavewordRepository.GetWords(Name, targetUser.Name);
            if (words.Count > 0 && !skip)
            {
                awaitingLeaveWords.Remove(targetName);
                awaitingLeaveWords.Add(targetName, message);
                return;
            }

            awaitingLeaveWords.Remove(targetName);
            await ServerDbContext.CreateAsync(new DbLeaveword
            {
                UserName = targetUser.Name,
                SendName = Name,
                Time = DateTime.Now.ToString("yyyyMMddHHmmss"),
                Words = message,
                Lookface = Mesh,
            });
        }

        #endregion

        #region User Secondary Password

        public ulong SecondaryPassword
        {
            get => user.PasswordId;
            set => user.PasswordId = value;
        }

        public bool IsUnlocked()
        {
            return SecondaryPassword == 0 || VarData[0] != 0;
        }

        public void UnlockSecondaryPassword()
        {
            VarData[0] = 1;
        }

        public bool CanUnlock2ndPassword()
        {
            return VarData[1] <= 2;
        }

        public void Increment2ndPasswordAttempts()
        {
            VarData[1] += 1;
        }

        public async Task SendSecondaryPasswordInterfaceAsync()
        {
            await GameAction.ExecuteActionAsync(8003020, this, null, null, string.Empty);
        }

        #endregion

        #region Cool Action

        private readonly TimeOut coolSyncTimer = new(5);

        public bool IsCoolEnable()
        {
            return coolSyncTimer.ToNextTime();
        }

        public bool IsFullSuper()
        {
            for (ItemPosition pos = ItemPosition.EquipmentBegin; pos <= ItemPosition.EquipmentEnd; pos++)
            {
                ItemBase item = GetEquipment(pos);
                if (item == null)
                {
                    switch (pos)
                    {
                        case ItemPosition.Sprite:
                        case ItemPosition.Crown:
                        case ItemPosition.Horn:
                        case ItemPosition.Rune:
                            continue;
                        default:
                            return false;
                    }
                }

                if (!item.IsEquipment())
                {
                    continue;
                }

                if (item.GetQuality() % 10 < 4)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsArmorSuper()
        {
            return GetEquipment(ItemPosition.Armor)?.GetQuality() >= 4;
        }

        #endregion

        #region Synchronization

        public override async Task<bool> AddAttributesAsync(ClientUpdateType type, long value)
        {
            bool screen = false,
                 save = false;

            switch (type)
            {
                case ClientUpdateType.Level:
                    {
                        if (value < 0)
                        {
                            return false;
                        }

                        screen = true;
                        value = Level = (byte)Math.Max(1, Math.Min(MAX_UPLEV, Level + value));

                        save = true;
                        await GameAction.ExecuteActionAsync(USER_UPLEV_ACTION, this, null, null, string.Empty);
                        break;
                    }

                case ClientUpdateType.Experience:
                    {
                        if (value < 0)
                        {
                            Experience = Math.Max(0, Experience - (ulong)(value * -1));
                        }
                        else
                        {
                            Experience += (ulong)value;
                        }

                        value = (long)Experience;
                        break;
                    }

                case ClientUpdateType.Force:
                    {
                        if (value < 0)
                        {
                            return false;
                        }

                        int maxAddPoints = MAX_ATTRIBUTE_POINTS - TotalAttributePoints;
                        if (maxAddPoints < 0)
                        {
                            return false;
                        }

                        value = Math.Min(maxAddPoints, value);
                        value = Strength = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Strength + value));
                        save = true;
                        break;
                    }
                case ClientUpdateType.Speed:
                    {
                        if (value < 0)
                        {
                            return false;
                        }

                        int maxAddPoints = MAX_ATTRIBUTE_POINTS - TotalAttributePoints;
                        if (maxAddPoints < 0)
                        {
                            return false;
                        }
                        value = Math.Min(maxAddPoints, value);
                        value = Speed = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Speed + value));
                        save = true;
                        break;
                    }

                case ClientUpdateType.Health:
                    {
                        if (value < 0)
                        {
                            return false;
                        }

                        int maxAddPoints = MAX_ATTRIBUTE_POINTS - TotalAttributePoints;
                        if (maxAddPoints < 0)
                        {
                            return false;
                        }

                        value = Math.Min(maxAddPoints, value);
                        value = Vitality = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Vitality + value));
                        save = true;
                        break;
                    }

                case ClientUpdateType.Spirit:
                    {
                        if (value < 0)
                        {
                            return false;
                        }

                        int maxAddPoints = MAX_ATTRIBUTE_POINTS - TotalAttributePoints;
                        if (maxAddPoints < 0)
                        {
                            return false;
                        }
                        value = Math.Min(maxAddPoints, value);
                        value = Spirit = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Spirit + value));
                        save = true;
                        break;
                    }

                case ClientUpdateType.Xp:
                    {
                        if (value < 0)
                        {
                            XpPoints = (byte)Math.Max(0, XpPoints - value * -1);
                        }
                        else
                        {
                            XpPoints = (byte)Math.Max(0, Math.Min(XpPoints + value, XPTIME_MAXVALUE));
                        }

                        value = XpPoints;
                        break;
                    }

                case ClientUpdateType.Energy:
                    {
                        if (value < 0)
                        {
                            Energy = (byte)Math.Max(0, Energy - value * -1);
                        }
                        else
                        {
                            Energy = (byte)Math.Max(0, Math.Min(MaxEnergy, Energy + value));
                        }

                        value = Energy;
                        break;
                    }

                case ClientUpdateType.MaxEnergy:
                    {
                        break;
                    }
                    
                case ClientUpdateType.PkPoints:
                    {
                        value = PkPoints = (ushort)Math.Max(0, Math.Min(PkPoints + value, ushort.MaxValue));
                        await CheckPkStatusAsync();
                        screen = true;
                        save = true;
                        break;
                    }

                case ClientUpdateType.Life:
                    {
                        value = Life = (uint)Math.Min(MaxLife, Math.Max(Life + value, 0));
                        //await BroadcastTeamLifeAsync();
                        break;
                    }

                case ClientUpdateType.Mana:
                    {
                        value = Mana = (uint)Math.Min(MaxLife, Math.Max(Mana + value, 0));
                        break;
                    }
                default:
                    {
                        return await base.AddAttributesAsync(type, value);
                    }
            }

            if (save)
            {
                await SaveAsync();
            }

            await SynchroAttributesAsync(type, (ulong)value, screen);
            return true;
        }

        public override async Task<bool> SetAttributesAsync(ClientUpdateType type, ulong value)
        {
            bool screen = false,
                save = false;
            switch (type)
            {
                case ClientUpdateType.Level:
                    {
                        save = screen = true;
                        Level = (byte)Math.Max(1, Math.Min(MAX_UPLEV, value));
                        break;
                    }

                case ClientUpdateType.Experience:
                    {
                        Experience = Math.Max(0, value);
                        break;
                    }

                case ClientUpdateType.MaxEnergy:
                    {
                        break;
                    }

                case ClientUpdateType.Xp:
                    {
                        XpPoints = (byte)Math.Max(0, Math.Min(value, 100));
                        break;
                    }

                case ClientUpdateType.Energy:
                    {
                        Energy = (byte)Math.Max(0, Math.Min(value, MaxEnergy));
                        break;
                    }

                case ClientUpdateType.Atributes:
                    {
                        save = true;
                        AttributePoints = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, value));
                        break;
                    }

                case ClientUpdateType.PkPoints:
                    {
                        PkPoints = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, value));
                        await CheckPkStatusAsync();
                        break;
                    }

                case ClientUpdateType.Mesh:
                    {
                        screen = true;
                        save = true;
                        Mesh = (uint)value;
                        break;
                    }

                case ClientUpdateType.HairStyle:
                    {
                        screen = true;
                        save = true;
                        Hairstyle = (ushort)value;
                        break;
                    }

                case ClientUpdateType.Force:
                    {
                        save = true;
                        value = Strength = (ushort)Math.Min(ushort.MaxValue, value);
                        break;
                    }

                case ClientUpdateType.Speed:
                    {
                        save = true;
                        value = Speed = (ushort)Math.Min(ushort.MaxValue, value);
                        break;
                    }

                case ClientUpdateType.Health:
                    {
                        save = true;
                        value = Vitality = (ushort)Math.Min(ushort.MaxValue, value);
                        break;
                    }

                case ClientUpdateType.Spirit:
                    {
                        save = true;
                        value = Spirit = (ushort)Math.Min(ushort.MaxValue, value);
                        break;
                    }

                case ClientUpdateType.Class:
                    {
                        save = true;
                        screen = true;
                        Profession = (byte)value;
                        break;
                    }

                case ClientUpdateType.Reborn:
                    {
                        save = true;
                        Metempsychosis = (byte)Math.Min(1, Math.Max(0, value));
                        value = Math.Min(1, value);
                        break;
                    }

                case ClientUpdateType.Money:
                    {
                        Silvers = (uint)Math.Max(0, Math.Min(int.MaxValue, value));
                        break;
                    }

                case ClientUpdateType.EudemonPoints:
                    {
                        EudemonPoints = (uint)Math.Max(0, Math.Min(int.MaxValue, value));
                        break;
                    }

                case ClientUpdateType.EudemonPointsBound:
                    {
                        EudemonPointsBound = (uint)Math.Max(0, Math.Min(int.MaxValue, value));
                        break;
                    }

                case ClientUpdateType.Life:
                    {
                        Life = (uint)Math.Min(value, MaxLife);
                        screen = true;
                        //await BroadcastTeamLifeAsync();
                        break;
                    }

                case ClientUpdateType.Mana:
                    {
                        Mana = (uint)Math.Min(value, MaxMana);
                        break;
                    }

                case ClientUpdateType.MaxLife:
                    {
                        break;
                    }

                default:
                    {
                        return await base.SetAttributesAsync(type, value);
                    }
            }

            if (save)
            {
                await SaveAsync();
            }
            await SynchroAttributesAsync(type, value, screen);
            return true;
        }

        public async Task CheckPkStatusAsync()
        {
            if (PkPoints > 99 && QueryStatus(StatusSet.STATUS_PKVALUEBLACK) == null)
            {
                await DetachStatusAsync(StatusSet.STATUS_PKVALUERED);
                await AttachStatusAsync(this, StatusSet.STATUS_PKVALUEBLACK, 0, int.MaxValue, 1);
            }
            else if (PkPoints > 29 && PkPoints < 100 && QueryStatus(StatusSet.STATUS_PKVALUERED) == null)
            {
                await DetachStatusAsync(StatusSet.STATUS_PKVALUEBLACK);
                await AttachStatusAsync(this, StatusSet.STATUS_PKVALUERED, 0, int.MaxValue, 1);
            }
            else if (PkPoints < 30)
            {
                await DetachStatusAsync(StatusSet.STATUS_PKVALUEBLACK);
                await DetachStatusAsync(StatusSet.STATUS_PKVALUERED);
            }
        }

        #endregion

        #region Home

        public uint HomeIdentity
        {
            get => user?.HomeId ?? 0u;
            set => user.HomeId = value;
        }

        #endregion

        #region Requests

        private readonly ConcurrentDictionary<RequestType, uint> requests = new();
        private int requestData;

        public void SetRequest(RequestType type, uint target, int data = 0)
        {
            requests.TryRemove(type, out _);
            if (target == 0)
            {
                return;
            }

            requestData = data;
            requests.TryAdd(type, target);
        }

        public uint QueryRequest(RequestType type)
        {
            return requests.TryGetValue(type, out uint value) ? value : 0;
        }

        public int QueryRequestData(RequestType type)
        {
            if (requests.TryGetValue(type, out _))
            {
                return requestData;
            }

            return 0;
        }

        public uint PopRequest(RequestType type)
        {
            if (requests.TryRemove(type, out uint value))
            {
                requestData = 0;
                return value;
            }
            return 0;
        }

        #endregion

        #region Logger

        public string GetDefaultLoggerPrefix()
        {
            return $"{Identity},{Name},{Level},{Metempsychosis},{Profession}";
        }

        #endregion

        #region Process Tick

        private uint firstClientTick = 0;
        private uint lastClientTick = 0;
        private uint lastRcvClientTick = 0;
        private Queue<uint> m_dequeServerTick = new();

        public async Task ProcessTickAsync(uint clientTime)
        {
            if (firstClientTick == 0)
            {
                firstClientTick = clientTime;
            }

            if (clientTime < lastClientTick)
            {
                logger.Error("Ridiculous time stamp, kicking client!");
                return;
            }

            const int CRITICAL_TICK_PERCENT = 5;
            int nServerTicks = m_dequeServerTick.Count;
            if (lastClientTick != 0 && nServerTicks >= 2 &&
                clientTime > lastClientTick + TICK_SECS * (100 + CRITICAL_TICK_PERCENT) * 10)
            {
                // suspicious time stamp
                int dwTimeServer = Environment.TickCount;
                int dwTimeServerTickInterval = (int)(m_dequeServerTick.Peek() - m_dequeServerTick.Last());
                int nEchoTime = (int)(dwTimeServer - m_dequeServerTick.Peek());
                if (nEchoTime < clientTime - lastClientTick - dwTimeServerTickInterval)
                {
                    logger.Warning($"Suspicious behavior detected: {Name}");
                    await RoleManager.KickOutAsync(Identity, "Connection Off");
                    return;
                }
            }

            lastClientTick = clientTime;
            if (m_dequeServerTick.Count >= 2)
            {
                m_dequeServerTick.Dequeue();
            }
            lastRcvClientTick = (uint)Environment.TickCount;
        }

        #endregion

        #region User Delete

        private bool deleted;

        public async Task<bool> DeleteUserAsync()
        {
            user.LogoutTime = (uint)UnixTimestamp.Now;
            await SaveAsync();

            await using var ctx = new ServerDbContext();
            await ctx.Database.BeginTransactionAsync();
            try
            {
                await OnUserDeletedAsync(this, ctx);

                await ctx.Database.ExecuteSqlRawAsync($"INSERT INTO `cq_deluser` SELECT * FROM `cq_user` WHERE id = {Identity} LIMIT 1;");

                ctx.Users.Remove(user);

                await ctx.SaveChangesAsync();
                await ctx.Database.CommitTransactionAsync();

                deleted = true;
            }
            catch (Exception ex)
            {
                await ctx.Database.RollbackTransactionAsync();
                logger.Error(ex, "Error on delete user: {0}", ex.Message);
                return false;
            }

            logger.Information("Deleted user {0} {1}", Identity, Name);
            return true;
        }

        #endregion

        #region Session

        public EOActionType LoginStage { get; set; }

        public DateTime? PreviousLoginTime { get; private set; }

        public DateTime? LastLogout
        {
            get => UnixTimestamp.ToNullableDateTime(user.LogoutTime);
            set => user.LogoutTime = (uint)UnixTimestamp.FromDateTime(value);
        }

        public async Task OnLoginAsync()
        {
            if (user.LoginTime == 0)
            {
                // on the future we will use lua, so let the system there.
                //LuaScriptManager.Run(this, null, null, null, "System_PlayLoginFirst()");
                user.LoginTime = UnixTimestamp.Now;
            }

            await DoDailyResetAsync(true);
            await GameAction.ExecuteActionAsync(1000000, this, null, null);
            await GameAction.ExecuteActionAsync(2000801, this, null, null);
            
            if (user.LastLoginTime > 0)
            {
                PreviousLoginTime = UnixTimestamp.ToDateTime(user.LastLoginTime);
            }

            if (EudemonPoints > 0)
            {
                uint currentCheckSum = CalculateEmoneyCheckSum(user.Emoney, user.Identity);
                if (user.EmoneyChkSum != currentCheckSum)
                {
                    logger.Warning("User {0} {1} has invalid value {2} of emoney. Last registered checksum {3} and current is {4}", Identity, Name, EudemonPoints, user.EmoneyChkSum, currentCheckSum);
                    await SetAttributesAsync(ClientUpdateType.EudemonPoints, 0);
                }
            }

            //await LoadTitlesAsync();
            //await SendMerchantAsync();
            await LoadLeaveWordAsync();
            //await LoadStatusAsync();

            LoadExperienceData();
            await SendMultipleExpAsync();

            await Screen.SynchroScreenAsync();

            user.LastLoginTime = (uint)UnixTimestamp.Now;
            await UserRepository.UpdateAsync(user);
        }

        public async Task OnLogoutAsync()
        {
            try
            {
                if (!deleted)
                {
                    await OnUserLogoutAsync(this);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on module logout. {0}", ex.Message);
            }

            try
            {
                if (!deleted)
                {
                    if (Map?.IsRecordDisable() == false)
                    {
                        if (IsAlive)
                        {
                            user.MapId = idMap;
                            user.X = currentX;
                            user.Y = currentY;
                        }
                    }
                }

                await LeaveMapAsync();
                if (!deleted)
                {
                    user.LogoutTime = (uint)UnixTimestamp.Now;
                    await UserRepository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on user logout! {0}", ex.Message);
            }
            finally
            {
                logger.Information("User {1} {0} has disconnected", Name, Identity);
                RoleManager.ForceLogoutUser(Identity);
            }
        }

        public async Task DoDailyResetAsync(bool login)
        {
            if (login && (!PreviousLoginTime.HasValue || PreviousLoginTime.Value.Date >= DateTime.Now.Date || LastLogout?.Date >= DateTime.Now.Date))
            {
                // already reseted
                return;
            }

            //if (!login)
            //{
            //    Statistic.ClearDailyStatistic();

            //    if (TaskDetail != null)
            //    {
            //        await TaskDetail.DailyResetAsync();
            //    }
            //}
        }

        #endregion

        #region Timer

        public async Task OnBattleTimerAsync()
        {
            
        }

        public override async Task OnTimerAsync()
        {
            if (Map == null)
            {
                return;
            }

            await base.OnTimerAsync();

            // the tick packet is something to work on, this does the checks for cheating on client
            // something we may use or not, idk if this is something to spent memory.
            //if (tickTimer.ToNextTime())
            //{
            //    await SendAsync(new MsgTick(Identity));
            //}

            if (Booth != null)
            {
                QueueAction(Booth.CheckBoothRentalAsync);
                if (Booth.CheckAdvertiseTimeOut())
                {
                    Booth.SetAdvertise(0);
                    NpcManager.RemBoothAdvertise(Booth.Identity);
                }
            }

            if (PkPoints > 0 && pkDecreaseTimer.ToNextTime(PK_DEC_TIME))
            {
                QueueAction(async () =>
                {
                    if (Map?.IsPrisionMap() == true)
                    {
                        await AddAttributesAsync(ClientUpdateType.PkPoints, PKVALUE_DEC_ONCE_IN_PRISON);
                    }
                    else
                    {
                        await AddAttributesAsync(ClientUpdateType.PkPoints, PKVALUE_DEC_ONCE);
                    }
                });
            }

            //if (Team != null)
            //{
            //    if (Team.IsLeader(Identity))
            //    {
            //        await Team.BroadcastLeaderPosAsync();
            //    }
            //}

            QueueAction(UserPackage.OnTimerAsync);

            try
            {
                if (StatusSet.Status.Count > 0)
                {
                    foreach (IStatus stts in StatusSet.Status.Values)
                    {
                        QueueAction(async () =>
                        {
                            await stts.OnTimerAsync();
                            if (!stts.IsValid && stts.Identity != StatusSet.STATUS_GHOST && stts.Identity != StatusSet.STATUS_DIE)
                            {
                                await StatusSet.DelObjAsync(stts.Identity);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Character::OnTimerAsync() => {Identity}:{Name} Status: {ex.Message}");
            }

            if (!IsAlive)
            {
                return;
            }

            if (Transformation != null && transformationTimer.IsActive() && transformationTimer.IsTimeOut())
            {
                QueueAction(ClearTransformationAsync);
            }

            if (Energy < MaxEnergy)
            {
                if (energyTimer.ToNextTime(ADD_ENERGY_STAND_MS))
                {
                    QueueAction(() => AddAttributesAsync(ClientUpdateType.Energy, ADD_ENERGY_STAND));
                }
            }

            if (xpPointsTimer.ToNextTime())
            {
                QueueAction(ProcXpValAsync);
            }

            if(clearXpTimer.IsActive() && clearXpTimer.ToNextTime())
            {
                clearXpTimer.Clear();
                await ClsXpValAsync();
            }

            if (setSlowHealUp2Life.Count > 0 && slowHealLife.ToNextTime())
            {
                QueueAction(ProcSlowHealLifeUpAsync);
            }

            if (setSlowHealUp2Mana.Count > 0 && slowHealMana.ToNextTime())
            {
                QueueAction(ProcSlowHealManaUpAsync);
            }
        }

        #endregion

        #region Socket

        public override Task SendAsync(IPacket msg)
        {
            return SendAsync(msg.Encode());
        }

        public override Task SendAsync(byte[] msg)
        {
            if (Client != null)
            {
                return Client.SendAsync(msg);
            }
            return Task.CompletedTask;
        }

        public async Task SendWindowToAsync(Character player)
        {
            await player.SendAsync(new MsgPlayer(this, player));
        }

        public override async Task SendSpawnToAsync(Character player)
        {
            await player.SendAsync(new MsgPlayer(this, player));
            //var mount = Mount;
            //if (mount != null)
            //{
            //    await player.SendAsync(new MsgDataArray()
            //    {
            //        Action = MsgDataArray.DataArrayMode.Mount,
            //        Param4 = new uint[6]
            //        {
            //            Identity,
            //            mount.Type,
            //            mount.Identity,
            //            (uint)(mount.EudemonStarLevel / 100), // only mounts with 10 stars, even if you send this without 10 stars, the mount is not showed.
            //            0, // is divine (boolean)
            //            0, // divine level
            //        }
            //    });
            //}
        }

        public override async Task SendSpawnToAsync(Character player, int x, int y)
        {
            await player.SendAsync(new MsgPlayer(this, player, (ushort)x, (ushort)y));
            //var mount = Mount;
            //if (mount != null)
            //{
            //    await player.SendAsync(new MsgDataArray()
            //    {
            //        Action = MsgDataArray.DataArrayMode.Mount,
            //        Param4 = new uint[6]
            //        {
            //            Identity,
            //            mount.Type,
            //            mount.Identity,
            //            (uint)(mount.EudemonStarLevel / 100), // only mounts with 10 stars, even if you send this without 10 stars, the mount is not showed.
            //            0, // is divine (boolean)
            //            0, // divine level
            //        }
            //    });
            //}
        }

        #endregion

        #region Database

        public Task SaveAsync()
        {
            return ServerDbContext.UpdateAsync(user);
        }

        #endregion

        public enum EmoneyOperationType
        {
            None,
            Npc,
            Trade,
            Booth,
            Item,
            Monster,
            EmoneyShop,
            Nobility,
            ChestPackage,
            SuperFlag,
            WeaponSkillUpgrade,
            DegradeItem,
            Syndicate,
            Pigeon,
            AuctionBid,
            AuctionBuy,
            Mail,
            Lua,
            AwardCommand
        }

        /// <summary>Enumeration type for body types for player characters.</summary>
        public enum BodyType : ushort
        {
            Male = 1,
            Female = 2
        }

        /// <summary>Enumeration type for base classes for player characters.</summary>
        public enum BaseClassType : ushort
        {
            Mage = 10,
            Warrior = 20,
            Paladin = 30,
            Vampire = 50,
            Necromancer = 60,
            ShadowKnight = 70
        }

        public enum PkModeType
        {
            FreePk,
            Peace,
            Team,
            Capture
        }

        [Flags]
        public enum VipFlags
        {
            VipOne = ItemStatusExtraTime | Friends | BlessTime,
            VipTwo = VipOne | BonusLottery | VipFurniture | CityTeleport,
            VipThree = VipTwo | PortalTeleport | CityTeleportTeam,
            VipFour = VipThree | Avatar | DailyQuests | VipHairStyles,
            VipFive = VipFour | FrozenGrotto,
            VipSix = PortalTeleport | Avatar | MoreForVip | FrozenGrotto | TeleportTeam
                      | CityTeleport | CityTeleportTeam | BlessTime | OfflineTrainingGround | ItemStatusExtraTime
                      | Friends | VipHairStyles | Labirint | DailyQuests | VipFurniture | BonusLottery,

            PortalTeleport = 0x1,
            Avatar = 0x2,
            MoreForVip = 0x4,
            FrozenGrotto = 0x8,
            TeleportTeam = 0x10,
            CityTeleport = 0x20,
            CityTeleportTeam = 0x40,
            BlessTime = 0x80,
            OfflineTrainingGround = 0x100,
            /// <summary>
            /// Refinery and Artifacts
            /// </summary>
            ItemStatusExtraTime = 0x200,
            Friends = 0x400,
            VipHairStyles = 0x800,
            Labirint = 0x1000,
            DailyQuests = 0x2000,
            VipFurniture = 0x4000,
            BonusLottery = 0x8000,

            None = 0
        }

        public enum PlayerCountry
        {
            UnitedArabEmirates = 1,
            Argentine,
            Australia,
            Belgium,
            Brazil,
            Canada,
            China,
            Colombia,
            CostaRica,
            CzechRepublic,
            Conquer,
            Germany,
            Denmark,
            DominicanRepublic,
            Egypt,
            Spain,
            Estland,
            Finland,
            France,
            UnitedKingdom,
            HongKong,
            Indonesia,
            India,
            Israel,
            Italy,
            Japan,
            Kuwait,
            SriLanka,
            Lithuania,
            Mexico,
            Macedonia,
            Malaysia,
            Netherlands,
            Norway,
            NewZealand,
            Peru,
            Philippines,
            Poland,
            PuertoRico,
            Portugal,
            Palestine,
            Qatar,
            Romania,
            Russia,
            SaudiArabia,
            Singapore,
            Sweden,
            Thailand,
            Turkey,
            UnitedStates,
            Venezuela,
            Vietnam = 52
        }

        public enum RequestType
        {
            Friend,
            InviteSyndicate,
            JoinSyndicate,
            TeamApply,
            TeamInvite,
            Trade,
            Marriage,
            TradePartner,
            Guide,
            Family,
            CoupleInteraction,
            ApplySynAlly,
        }
    }
}
