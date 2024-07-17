using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.Processors;
using Long.Kernel.States.Status;
using Long.Kernel.States.User;
using Long.Kernel.States.World;
using Long.Network.Packets;
using Long.Shared.Managers;
using Long.Shared.Mathematics;
using Long.World.Map;
using Long.World.Roles;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;
using static Long.Kernel.Network.Game.Packets.MsgName;
using static Long.Kernel.Network.Game.Packets.MsgWalkEx;

namespace Long.Kernel.States
{
    public abstract class Role : WorldObject
    {
        private static readonly ILogger logger = Log.ForContext<Role>();

        protected uint idMap;
        protected uint maxLife = 0, maxMana = 0;
        protected ushort currentX, currentY;

        public virtual uint OwnerIdentity { get; set; }
        public virtual uint Mesh { get; set; }
        public virtual byte Level { get; set; }
        public virtual int Profession { get; set; }
        public virtual bool HasGenerator { get; protected set; } = false;

        #region Life and Mana

        public virtual uint Life { get; set; }
        public virtual uint MaxLife => maxLife;
        public virtual uint Mana { get; set; }
        public virtual uint MaxMana => maxMana;
        public virtual byte Energy { get; set; }

        #endregion

        #region Role Type

        public bool IsPlayer()
        {
            return Identity >= IdentityManager.PLAYER_ID_FIRST && Identity < IdentityManager.PLAYER_ID_LAST;
        }

        public bool IsMonster()
        {
            return Identity >= IdentityManager.MONSTERID_FIRST && Identity < IdentityManager.MONSTERID_LAST;
        }

        public bool IsEudemon()
        {
            return Identity >= IdentityManager.EUDEMON_ID_FIRST && Identity < IdentityManager.EUDEMON_ID_LAST;
        }

        public bool IsNpc()
        {
            return Identity >= IdentityManager.SYSNPCID_FIRST && Identity < IdentityManager.SYSNPCID_LAST;
        }

        public bool IsDynaNpc()
        {
            return Identity >= IdentityManager.DYNANPCID_FIRST && Identity < IdentityManager.DYNANPCID_LAST;
        }

        public bool IsCallPet()
        {
            return IsEudemon() || (Identity >= IdentityManager.CALLPETID_FIRST && Identity < IdentityManager.CALLPETID_LAST);
        }

        public bool IsTrap()
        {
            return Identity >= IdentityManager.TRAPID_FIRST && Identity < IdentityManager.TRAPID_LAST;
        }

        public bool IsMapItem()
        {
            return Identity >= IdentityManager.MAPITEM_FIRST && Identity < IdentityManager.MAPITEM_LAST;
        }

        public bool IsFurniture()
        {
            return Identity >= IdentityManager.SCENE_NPC_MIN && Identity < IdentityManager.SCENE_NPC_MAX;
        }

        #endregion

        #region Battle Attributes

        public virtual int BattlePower => 1;

        public virtual int Luck { get; } = 0;
        public virtual int MinAttack { get; } = 1;
        public virtual int MaxAttack { get; } = 1;
        public virtual int MagicAttackMin { get; } = 1;
        public virtual int MagicAttackMax { get; } = 1;
        public virtual int Defense { get; } = 0;
        public virtual int MagicDefense { get; } = 0;
        public virtual int Dodge { get; } = 0;
        public virtual int AttackSpeed { get; } = 1000;
        public virtual int Accuracy { get; } = 1;
        public virtual int Blessing { get; } = 0;

        public virtual int AddFinalAttack { get; } = 0;
        public virtual int AddFinalMAttack { get; } = 0;
        public virtual int AddFinalDefense { get; } = 0;
        public virtual int AddFinalMDefense { get; } = 0;

        public virtual int CriticalStrike => 0;
        public virtual int SkillCriticalStrike => 0;
        public virtual int Immunity => 0;
        public virtual int Penetration => 0;
        public virtual int Breakthrough => 0;
        public virtual int Counteraction => 0;
        public virtual int Block => 0;
        public virtual int Detoxication => 0;
        public virtual int FireResistance => 0;
        public virtual int WaterResistance => 0;
        public virtual int WoodResistance => 0;
        public virtual int EarthResistance => 0;
        public virtual int MetalResistance => 0;

        public virtual int ExtraDamage { get; } = 0;
        public virtual int Potential { get; set; } = 0;
        #endregion

        #region Battle

        //public BattleSystem BattleSystem { get; init; }
        //public MagicData MagicData { get; init; }

        public virtual bool IsAlive => Life > 0;
        public virtual bool IsBowman => false;
        public virtual bool IsShieldUser => false;
        public virtual int SizeAddition => 1;
        public virtual int Defense2 => 10000;
        public virtual bool IsSimpleMagicAtk() => false;

        public virtual bool IsEmbedGemType(uint type)
        {
            return false;
        }

        public virtual bool SetAttackTarget(Role target)
        {
            return true;
        }

        public virtual Task<bool> CheckCrimeAsync(Role target)
        {
            return Task.FromResult(false);
        }

        public virtual int AdjustWeaponDamage(int damage, Role target)
        {
            return Calculations.MulDiv(damage, Defense2, Calculations.DEFAULT_DEFENCE2);
        }

        public virtual int AdjustMagicDamage(int nDamage)
        { 
            return nDamage;
        }

        public virtual int GetAttackRange(int sizeAdd)
        {
            return sizeAdd + 1;
        }

        public virtual bool IsAttackable(Role attacker)
        {
            return true;
        }

        public virtual bool IsImmunity(Role target)
        {
            if (target == null)
            {
                return true;
            }

            if (target.Identity == Identity)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsFarWeapon()
        {
            return false;
        }

        public virtual Task<bool> TransferShieldAsync(bool bMagic, Role pAtker, int nDamage)
        {
            return Task.FromResult(false);
        }

        public virtual Task AdditionMagicAsync(int lifeLost, int damage)
        {
            return Task.CompletedTask;
        }

        public virtual int AdjustAttack(int nRawAtk)
        {
            return nRawAtk;
        }

        public virtual int AdjustDefence(int nDef)
        {
            return nDef;
        }

        public virtual int AdjustMagicAtk(int nAtk)
        {
            return nAtk;
        }

        public virtual int AdjustMagicDef(int nDef)
        {
            return nDef;
        }

        public virtual int AdjustExp(Role pTarget, int nRawExp, bool bNewbieBonusMsg = false)
        {
            return nRawExp;
        }

        public virtual Task AwardExpForEudemonAsync(Role target, int exp)
        {
            return Task.CompletedTask;
        }

        //public virtual Task<AttackResult> AttackAsync(Role target)
        //{
        //    return Task.FromResult<AttackResult>(new());
        //}

        //public virtual Task<bool> BeAttackAsync(Magic.MagicType magicType, Role attacker, int power, bool reflectEnabled)
        //{
        //    return Task.FromResult(false);
        //}

        public virtual Task BeKillAsync(Role attacker)
        {
            return Task.CompletedTask;
        }

        public virtual async Task KillAsync(Role target, uint dieWay)
        {
            //if (this is Monster guard && guard.IsGuard())
            //{
            //    await BroadcastRoomMsgAsync(new MsgInteract
            //    {
            //        Action = MsgInteractType.Kill,
            //        SenderIdentity = Identity,
            //        TargetIdentity = target.Identity,
            //        PosX = target.X,
            //        PosY = target.Y,
            //        Data = (int)dieWay
            //    }, true);
            //}

            await target.BeKillAsync(this);
        }

        public virtual Task<bool> AutoSkillAttackAsync(Role target)
        {
            return Task.FromResult(false);
        }

        public Task<bool> ProcessMagicAttackAsync(ushort usMagicType, uint idTarget, ushort x, ushort y, uint autoActive = 0)
        {
            return Task.FromResult(false);
        }

        public async Task SendDamageMsgAsync(uint idTarget, int damage)
        {
            //await Map.BroadcastRoomMsgAsync(X, Y, new MsgInteract
            //{
            //    SenderIdentity = Identity,
            //    TargetIdentity = idTarget,
            //    Data = damage,
            //    Command = damage,
            //    PosX = X,
            //    PosY = Y,
            //    Action = IsBowman ? MsgInteractType.Shoot : MsgInteractType.Attack,
            //});
        }

        #endregion

        #region Map and position

        public virtual GameMap Map { get; protected set; }

        public int Partition => Map?.Partition ?? -1;

        /// <summary>
        ///     The current map identity for the role.
        /// </summary>
        public virtual uint MapIdentity
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
        ///     Current Y position of the user in the map.
        /// </summary>
        public override ushort Y
        {
            get => currentY;
            set => currentY = value;
        }

        public int GetDistance(int x, int y)
        {
            return Calculations.GetDistance(X, Y, x, y);
        }

        public int GetDistance(Role target)
        {
            return GetDistance(target.X, target.Y);
        }

        public virtual Task EnterMapAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task LeaveMapAsync()
        {
            return Task.CompletedTask;
        }

        public Role QueryRole(uint idRole)
        {
            return Map.QueryAroundRole(this, idRole);
        }

        #endregion

        #region Movement

        public bool IsJumpPass(int x, int y, int alt)
        {
            List<Point> setLine = new();
            Calculations.DDALineEx(X, Y, x, y, ref setLine);
            if (setLine.Count <= 2)
            {
                return true;
            }

            if (x != setLine[^1].X)
            {
                return false;
            }

            if (y != setLine[^1].Y)
            {
                return false;
            }

            int currentAltitude = Map.GetFloorAlt(X, Y);
            int lastAltitude = Map.GetFloorAlt(x, y);
            if (lastAltitude - currentAltitude > alt)
            {
                return false;
            }

            foreach (Point point in setLine)
            {
                int nextCellAltitude = Map.GetFloorAlt(point.X, point.Y);
                int difference = nextCellAltitude - currentAltitude;
                if (difference > alt && difference < 1000)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> JumpPosAsync(int x, int y, bool sync = false)
        {
            if (x == X && y == Y)
            {
                return false;
            }

            if (Map == null || !Map.IsValidPoint(x, y))
            {
                return false;
            }

            if (IsPlayer())
            {
                Character user = (Character)this;
                if (!Map.IsStandEnable(x, y) || !user.IsJumpPass(x, y, MAX_JUMP_ALTITUDE))
                {
                    await user.SendAsync(StrInvalidCoordinate, TalkChannel.TopLeft, Color.Red);
                    await user.KickbackAsync();
                    return false;
                }

                if (Map.IsRaceTrack())
                {
                    await user.KickbackAsync();
                    return false;
                }
            }

            Map.EnterBlock(this, x, y, X, Y);
            Direction = (FacingDirection)Calculations.GetDirectionSector(X, Y, x, y);
            if (sync)
            {
                await BroadcastRoomMsgAsync(new MsgAction
                {
                    CommandX = (ushort)x,
                    CommandY = (ushort)y,
                    ArgumentX = currentX,
                    ArgumentY = currentY,
                    Identity = Identity,
                    Action = EOActionType.actionJump,
                    Direction = (ushort)Direction
                }, true);
            }

            currentX = (ushort)x;
            currentY = (ushort)y;
            return true;
        }

        public async Task<bool> MoveTowardAsync(int direction, int mode, bool sync = false)
        {
            if (Map == null)
            {
                return false;
            }

            var user = this as Character;
            ushort newX = 0, newY = 0;
            ushort oldX = X, oldY = Y;
            if (mode == (int)RoleMoveMode.Track)
            {
                direction %= 24;
                newX = (ushort)(X + GameMapData.RideXCoords[direction]);
                newY = (ushort)(Y + GameMapData.RideYCoords[direction]);
            }
            else
            {
                direction %= 8;
                newX = (ushort)(X + GameMapData.WalkXCoords[direction]);
                newY = (ushort)(Y + GameMapData.WalkYCoords[direction]);

                bool isRunning = mode >= (int)RoleMoveMode.RunDir0 && mode <= (int)RoleMoveMode.RunDir7;
                if (isRunning && IsAlive)
                {
                    newX += (ushort)GameMapData.WalkXCoords[mode - (int)RoleMoveMode.RunDir0];
                    newY += (ushort)GameMapData.WalkYCoords[mode - (int)RoleMoveMode.RunDir0];
                }
            }

            if (!Map.IsMoveEnable(newX, newY) && user != null)
            {
                await user.KickbackAsync();
                await user.SendAsync(StrInvalidCoordinate, TalkChannel.TopLeft, Color.Red);
                return false;
            }
//#if DEBUG
//            //logger.Information($"Walking to X{newX} and Y{newY}");
//#endif
            Map.EnterBlock(this, newX, newY, X, Y);
            Direction = (FacingDirection)direction;
            if (sync)
            {
                await BroadcastRoomMsgAsync(new MsgWalkEx
                {
                    Identity = Identity,
                    CommandX = newX,
                    CommandY = newY,
                    Direction = (byte)direction,
                    Mode = (byte)mode,
                }, false);
            }

            currentX = newX;
            currentY = newY;

            return true;
        }

        public async Task<bool> JumpBlockAsync(int x, int y, FacingDirection dir)
        {
            int steps = GetDistance(x, y);
            if (steps <= 0)
            {
                return false;
            }

            for (var i = 0; i < steps; i++)
            {
                var pos = new Point(X + (x - X) * i / steps, Y + (y - Y) * i / steps);
                if (Map.IsStandEnable(pos.X, pos.Y))
                {
                    await JumpPosAsync(pos.X, pos.Y, true);
                    return true;
                }
            }

            if (Map.IsStandEnable(x, y))
            {
                await JumpPosAsync(x, y, true);
                return true;
            }

            return false;
        }

        public async Task<bool> FarJumpAsync(int x, int y, FacingDirection dir)
        {
            int steps = GetDistance(x, y);

            if (steps <= 0)
            {
                return false;
            }

            if (Map.IsStandEnable(x, y))
            {
                await JumpPosAsync(x, y, true);
                return true;
            }

            for (var i = 0; i < steps; i++)
            {
                var pos = new Point(X + (x - X) * i / steps, Y + (y - Y) * i / steps);
                if (Map.IsStandEnable(pos.X, pos.Y))
                {
                    await JumpPosAsync(pos.X, pos.Y, true);
                    return true;
                }
            }

            return false;
        }

        public virtual Task ProcessOnMoveAsync(MsgWalkEx.RoleMoveMode mode)
        {
            Action = EntityAction.Stand;
            return Task.CompletedTask;
        }

        public virtual Task ProcessAfterMoveAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task ProcessOnAttackAsync()
        {
            Action = EntityAction.Stand;
            return Task.CompletedTask;
        }

       public virtual async Task<  bool> SyncTrackToAsync(int nPosX, int nPosY, int nDir, uint dwAction)
        {
            Direction = (FacingDirection)(nDir % 8);
            if (X != nPosX || Y != nPosY)
            {
                // this code has been moved to JumpBlockAsync!
                //if (!Map.IsValidPoint(nPosX, nPosY))
                //{
                //    return false;
                //}

                //if (!Map.IsStandEnable(nPosX, nPosY))
                //{
                //    //SendSysMsg(STR_INVALID_COORDINATE);
                //    //::LogSave("Invalid coordinate (%d, %d)", nPosX, nPosY);
                //    return false;
                //}
                await DetachStatusAsync(StatusSet.STATUS_HIDDEN);
                await JumpBlockAsync(nPosX, nPosY, Direction);

                //CUser* pUser = NULL;
                //if (this->QueryObj(OBJ_USER, IPP_OF(pUser)))
                //{
                //    pUser->StandRestart();
                //    pMap->ChangeRegion(pUser, nPosX, nPosY);
                //}

                X = (ushort)nPosX;
                Y = (ushort)nPosY;
            }

            if (IsPlayer())
            {
                await (this as Character).Screen.UpdateAsync();
            }
            //	ProcessAfterMove();
            await BroadcastRoomMsgAsync(new MsgAction()
            {
                Action = EOActionType.actionMagicTrack,
                Identity = Identity,
                CommandX = X,
                CommandY = Y,
                Argument = (byte)Direction,
                Data = dwAction
            }, true);

            return true;
        }

        #endregion

        #region Action and Direction

        public virtual FacingDirection Direction { get; protected set; }

        public virtual EntityAction Action { get; protected set; }

        public virtual async Task SetDirectionAsync(FacingDirection direction, bool sync = true)
        {
            Direction = direction;
            if (sync)
            {
                await BroadcastRoomMsgAsync(new MsgAction
                {
                    Identity = Identity,
                    Action = EOActionType.actionChgDir,
                    Argument = (ushort)direction,
                    CommandX = X,
                    CommandY = Y
                }, true);
            }
        }

        public virtual async Task SetActionAsync(EntityAction action, bool sync = true)
        {
            Action = action;
            if (sync)
            {
                await BroadcastRoomMsgAsync(new MsgAction
                {
                    Identity = Identity,
                    Action = EOActionType.actionEmotion,
                    Argument = (byte)Direction,
                    Data = (ushort)action,
                    CommandX = X,
                    CommandY = Y
                }, true);
            }
        }

        #endregion

        #region Synchronization

        public virtual async Task<bool> AddAttributesAsync(ClientUpdateType type, long value)
        {
            long currAttr;
            switch (type)
            {
                case ClientUpdateType.Life:
                    currAttr = Life = (uint)Math.Min(MaxLife, Math.Max(Life + value, 0));
                    break;

                case ClientUpdateType.Mana:
                    currAttr = Mana = (uint)Math.Min(MaxMana, Math.Max(Mana + value, 0));
                    break;

                default:
                    logger.Warning("Role::AddAttributes {0} not handled", type);
                    return false;
            }

            await SynchroAttributesAsync(type, (ulong)currAttr);
            return true;
        }

        public virtual async Task<bool> SetAttributesAsync(ClientUpdateType type, ulong value)
        {
            switch (type)
            {
                case ClientUpdateType.Life:
                    value = Life = (uint)Math.Max(0, Math.Min(MaxLife, value));
                    break;

                case ClientUpdateType.Mana:
                    value = Mana = (uint)Math.Max(0, Math.Min(MaxMana, value));
                    break;

                default:
                    logger.Warning("Role::SetAttributes {0} not handled", type);
                    return false;
            }

            await SynchroAttributesAsync(type, value, !IsPlayer());
            return true;
        }

        public async Task SynchroAttributesAsync(ClientUpdateType type, ulong value, bool screen = false)
        {
            var msg = new MsgUserAttrib(Identity, type, value);
            if (IsPlayer())
            {
                await SendAsync(msg);
            }

            if (IsEudemon())
            {
                var user = RoleManager.GetUser(OwnerIdentity);
                if (user != null)
                {
                    await user.SendAsync(msg);
                }
            }

            if (screen && Map != null)
            {
                await Map.BroadcastRoomMsgAsync(X, Y, msg, Identity);
            }
        }

        public async Task SynchroAttributesAsync(ClientUpdateType type, uint value1, uint value2, uint value3, uint value4, bool screen = false)
        {
            var msg = new MsgUserAttrib(Identity, type, value1, value2, value3, value4);
            if (IsPlayer())
            {
                await SendAsync(msg);
            }

            if (screen && Map != null)
            {
                await Map.BroadcastRoomMsgAsync(X, Y, msg, Identity);
            }
        }

        #endregion

        #region Status

        protected const int maxStatus = 32 * 4;

        public virtual bool IsWing => QueryStatus(StatusSet.STATUS_FLY) != null;

        public bool IsGhost => QueryStatus(StatusSet.STATUS_DIE) != null;

        public uint[] StatusFlag { get; set; } = new uint[maxStatus / 32];

        public StatusSet StatusSet { get; init; }

        public virtual async Task<bool> DetachWellStatusAsync()
        {
            for (var i = 1; i < maxStatus; i++)
            {
                if (StatusSet[i] != null)
                {
                    if (IsWellStatus(i))
                    {
                        await DetachStatusAsync(i);
                    }
                }
            }

            return true;
        }

        public virtual async Task<bool> DetachBadlyStatusAsync()
        {
            for (var i = 1; i < maxStatus; i++)
            {
                if (StatusSet[i] != null)
                {
                    if (IsBadlyStatus(i))
                    {
                        await DetachStatusAsync(i);
                    }
                }
            }

            return true;
        }

        public virtual async Task<bool> DetachAllStatusAsync()
        {
            await DetachBadlyStatusAsync();
            await DetachWellStatusAsync();
            return true;
        }

        public static bool IsWellStatus(int stts)
        {
            switch (stts)
            {
            }

            return false;
        }

        public static bool IsBadlyStatus(int stts)
        {
            switch (stts)
            {
            }

            return false;
        }

        public virtual bool HasDebilitatingStatus()
        {
            foreach (var status in StatusSet.Status.Values)
            {
                switch (status.Identity)
                {
                }
            }
            return false;
        }

        public async Task<bool> AttachStatusAsync(DbStatus status)
        {
            if (Map == null)
            {
                return false;
            }

            if (status.LeaveTimes > 1)
            {
                var pNewStatus = new StatusMore
                {
                    Model = status
                };

                if (await pNewStatus.CreateAsync(this, (int)status.Status, status.Power, (int)status.RemainTime, (int)status.LeaveTimes))
                {
                    await StatusSet.AddObjAsync(pNewStatus);
                    return true;
                }
            }
            else
            {
                var pNewStatus = new StatusOnce
                {
                    Model = status
                };
                if (await pNewStatus.CreateAsync(this, (int)status.Status, status.Power, (int)status.RemainTime, 0))
                {
                    await StatusSet.AddObjAsync(pNewStatus);
                    return true;
                }
            }

            return false;
        }

        public Task<bool> AttachStatusAsync(int status, int power, int secs, int times, object magic = null, bool save = false)
        {
            return AttachStatusAsync(this, status, power, secs, times, magic, save);
        }

        public async Task<bool> AttachStatusAsync(Role sender, int status, int power, int secs, int times, object magic = null, bool save = false)
        {
            if (Map == null)
            {
                return false;
            }

            //if (QueryStatus(StatusSet.GODLY_SHIELD) != null && IsBadlyStatus(status))
            //{
            //    return false;
            //}

            IStatus pStatus = QueryStatus(status);
            if (pStatus != null)
            {
                var changeData = false;
                if (pStatus.Power == power)
                {
                    changeData = true;
                }
                else
                {
                    int minPower = Math.Min(power, pStatus.Power);
                    int maxPower = Math.Max(power, pStatus.Power);

                    if (power <= 30000)
                    {
                        changeData = true;
                    }
                    else
                    {
                        if (minPower >= 30100 || minPower > 0 && maxPower < 30000)
                        {
                            if (power > pStatus.Power)
                            {
                                changeData = true;
                            }
                        }
                        else if (maxPower < 0 || minPower > 30000 && maxPower < 30100)
                        {
                            if (power < pStatus.Power)
                            {
                                changeData = true;
                            }
                        }
                    }
                }

                if (changeData)
                {
                    await pStatus.ChangeDataAsync(power, secs, times, sender?.Identity ?? 0);
                }

                return true;
            }

            if (times > 1)
            {
                var newStatus = new StatusMore();
                if (await newStatus.CreateAsync(this, status, power, secs, times, sender?.Identity ?? 0, magic, save))
                {
                    await StatusSet.AddObjAsync(newStatus);
                    return true;
                }
            }
            else
            {
                var newStatus = new StatusOnce();
                if (await newStatus.CreateAsync(this, status, power, secs, 0, sender?.Identity ?? 0, magic, save))
                {
                    await StatusSet.AddObjAsync(newStatus);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DetachStatusAsync(int nType)
        {
            return await StatusSet.DelObjAsync(nType);
        }

        public virtual bool IsOnXpSkill()
        {
            return false;
        }

        public virtual IStatus QueryStatus(int nType)
        {
            var status = StatusSet?.GetObjByIndex(nType);
            if (status != null && status.IsValid)
            {
                return status;
            }
            return null;
        }

        #endregion

        #region Processor Queue

        public void QueueAction(Func<Task> task)
        {
            // do not queue actions if not in map
            if (Map != null)
            {
                WorldProcessor.Instance.Queue(Map.Partition, task);
            }
        }

        #endregion

        #region Timer

        public virtual Task OnTimerAsync()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Socket

        public async Task SendEffectAsync(string effect, bool self)
        {
            var msg = new MsgName
            {
                Identity = Identity,
                Action = StringAction.RoleEffect
            };
            msg.Strings.Add(effect);
            await Map.BroadcastRoomMsgAsync(X, Y, msg, self ? 0 : Identity);
        }

        public Task SendEffectAsync(Character target, string effect)
        {
            var msg = new MsgName
            {
                Identity = Identity,
                Action = StringAction.RoleEffect
            };
            msg.Strings.Add(effect);
            return target.SendAsync(msg);
        }

        public async Task SendAsync(string message, TalkChannel channel = TalkChannel.TopLeft, Color? color = null)
        {
            await SendAsync(new MsgTalk(channel, color ?? Color.Red, message));
        }

        public virtual Task SendAsync(IPacket msg)
        {
            logger.Warning($"{GetType().Name} - {Identity} has no SendAsync(IPacket) handler");
            return Task.CompletedTask;
        }

        public virtual Task SendAsync(byte[] msg)
        {
            logger.Warning($"{GetType().Name} - {Identity} has no SendAsync(byte[]) handler");
            return Task.CompletedTask;
        }

        public virtual Task SendSpawnToAsync(Character player)
        {
            logger.Warning($"{GetType().Name} - {Identity} has no SendSpawnToAsync handler");
            return Task.CompletedTask;
        }

        public virtual Task SendSpawnToAsync(Character player, int x, int y)
        {
            logger.Warning($"{GetType().Name} - {Identity} has no SendSpawnToAsync(player, x, y) handler");
            return Task.CompletedTask;
        }

        public virtual async Task BroadcastRoomMsgAsync(IPacket msg, bool self)
        {
            if (Map != null)
            {
                await Map.BroadcastRoomMsgAsync(X, Y, msg, !self ? Identity : 0);
            }
        }

        public virtual Task BroadcastRoomMsgAsync(string message, TalkChannel channel = TalkChannel.TopLeft, Color? color = null)
        {
            return BroadcastRoomMsgAsync(new MsgTalk(channel, color ?? Color.White, message), true);
        }

        #endregion

        public enum FacingDirection : byte
        {
            Begin = SouthEast,
            SouthWest = 0,
            West = 1,
            NorthWest = 2,
            North = 3,
            NorthEast = 4,
            East = 5,
            SouthEast = 6,
            South = 7,
            End = South,
            Invalid = End + 1
        }

        public enum EntityAction : ushort
        {
            None,
            Dance1 = 1,
            Dance2 = 2,
            Dance3 = 3,
            Dance4 = 4,
            Dance5 = 5,
            Dance6 = 6,
            Dance7 = 7,
            Dance8 = 8,
            Stand = 100,
            Happy = 150,
            Angry = 160,
            Sad = 170,
            Wave = 190,
            Bow = 200,
            Kneel = 210,
            Cool = 230,
            Sit = 250,
            Lie = 270,

            InteractionKiss = 34466,
            InteractionHold = 34468,
            InteractionHug = 34469,
            CoupleDances = 34474
        }

        public const uint USER_KILL_ACTION = 80_000_001;
        public const uint USER_DIE_ACTION = 80_000_003;
        public const uint USER_UPLEV_ACTION = 80_000_004;
        public const uint MONSTER_DIE_ACTION = 80_000_010;

        public const byte MAX_UPLEV = 143;
        public const byte MAX_USER_GODLEV = 50;


        public const int XPTIME_TICK = 3;
        public const int XPTIME_INCAMOUNT = 1;
        public const int XPTIME_MAXVALUE = 1;
        public const int XPTIME_KEEPEFFECT = 60;

        public const int EXPBALL_AMOUNT = 600;
        public const int CHGMAP_LOCK_SECS = 10;
        public const int ADD_ENERGY_STAND_MS = 1000;
        public const int ADD_ENERGY_STAND = 50;
        public const int ADD_ENERGY_SIT = 15;
        public const int ADD_ENERGY_LIE = ADD_ENERGY_SIT / 2;
        public const int DEFAULT_USER_ENERGY = 70;
        public const int MAX_USER_ATTRIB_POINTS = 900;
        public const int KEEP_STAND_MS = 1500;
        public const int MIN_SUPERMAP_KILLS = 25;
        public const int VETERAN_DIFF_LEVEL = 20;
        public const int HIGHEST_WATER_WIZARD_PROF = 135;
        public const int SLOWHEALLIFE_MS = 1000;
        public const int AUTOHEALLIFE_TIME = 10;
        public const int AUTOHEALLIFE_EACHPERIOD = 6;
        public const int TICK_SECS = 10;
        public const int MAX_PKLIMIT = 10000;
        public const int PILEMONEY_CHANGE = 5000;
        public const int ADDITIONALPOINT_NUM = 3;
        public const int PK_DEC_TIME = 360;
        public const int PKVALUE_DEC_ONCE = -1;
        public const int PKVALUE_DEC_ONCE_IN_PRISON = -3;
        public const int USER_ATTACK_SPEED = 1000;
        public const int POISONDAMAGE_INTERVAL = 2;
        public const long MAX_INVENTORY_MONEY = 10_000_000_000;
        public const long MAX_STORAGE_MONEY = 1_000_000_000;
        public const uint MAX_INVENTORY_EMONEY = 1_000_000_000;

        public const int MAX_STRENGTH_POINTS_VALUE = 4000; // Chi Points

        public const int MASTER_WEAPONSKILLLEVEL = 12;
        public const int MAX_WEAPONSKILLLEVEL = 20;

        public const int MAX_MENUTASKSIZE = 8;
        public const int MAX_VAR_AMOUNT = 16;

        public const int SYNWAR_PROFFER_PERCENT = 1;
        public const int SYNWAR_MONEY_PERCENT = 2;
        public const int SYNWAR_NOMONEY_DAMAGETIMES = 10;

        public const int MAX_ATTRIBUTE_POINTS = 900;

        public const int NPCDIEDELAY_SECS = 10;

        public const int MAX_JUMP_ALTITUDE = 0x64; // according to CHero::CanJump(C3_POS)

        public const int DEFAULT_EUDEMON_POTENTIAL = 50;
        public const int MAX_EUDEMON_POTENTIAL = 100;      // »ÃÊÞ×î´óÇ±ÄÜÖµ
        public const int ADD_POTENTIAL_KILLNUM = 100;      // Ã¿É±ËÀ100¸öÄ¿±êÔö¼ÓÇ±Á¦Öµ
        public const int ADD_POTENTIAL_PER_KILLNUM = 1;
        public const int ADD_POTENTIAL_LEVUP = 5;

        public const int ADD_RELATIONSHIP_KILLNUM = 100;       // Ã¿É±ËÀ100¸öÄ¿±êÔö¼Ó¹ØÏµÖµ
        public const int ADD_RELATIONSHIP_PER_TIME = 1;     // Ã¿´ÎÔö¼õµÄ¹ØÏµÖµ

        public const int TRAPID_FIRST = 900001;
        public const int MAGICTRAPID_FIRST = 900001;
        public const int MAGICTRAPID_LAST = 989999;
        public const int SYSTRAPID_FIRST = 990001;
        public const int SYSTRAPID_LAST = 999999;
        public const int TRAPID_LAST = 999999;
    }
}
