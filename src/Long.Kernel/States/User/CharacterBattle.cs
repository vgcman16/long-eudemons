using Long.Database.Entities;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.Scripting.Action;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.Status;
using Long.Shared.Mathematics;
using static Long.Kernel.Network.Game.Packets.MsgAction;
using static Long.Kernel.States.Items.ItemBase;

namespace Long.Kernel.States.User
{
    public partial class Character
    {
        #region Attributes

        public override int BattlePower
        {
            get
            {
                int result = Level;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.BattlePower ?? 0;
                }

                return result;
            }
        }

        public int PureBattlePower
        {
            get
            {
                int result = Level;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.BattlePower ?? 0;
                }

                return result;
            }
        }

        public override int MinAttack
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.MinAttack;
                }

                int result = Strength;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.MinAttack ?? 0;
                }

                return result;
            }
        }

        public override int MaxAttack
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.MaxAttack;
                }

                int result = Strength;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.MaxAttack ?? 0;
                }

                return result;
            }
        }

        public override int MagicAttackMin
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.MaxAttack;
                }

                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.MagicAttackMin ?? 0;
                }

                return result;
            }
        }

        public override int MagicAttackMax
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.MaxAttack;
                }

                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.MagicAttackMax ?? 0;
                }

                return result;
            }
        }

        public override int Defense
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.Defense;
                }

                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.Defense ?? 0;
                }

                return result;
            }
        }

        public override int MagicDefense
        {
            get
            {
                if (Transformation != null)
                {
                    return Transformation.MagicDefense;
                }

                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    if (pos == ItemBase.ItemPosition.Sprite)
                    {
                        continue;
                    }

                    result += UserPackage.GetEquipment(pos)?.MagicDefense ?? 0;
                }

                return result;
            }
        }

        public override int Dodge
        {
            get
            {
                if (Transformation != null)
                {
                    return (int)Transformation.Dodge;
                }

                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.Dodge ?? 0;
                }
                return result;
            }
        }

        public override int AttackSpeed { get; } = 1000;

        public int Agility
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.Agility ?? 0;
                }
                return result;
            }
        }

        public override int Accuracy
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.Accuracy ?? 0;
                }
                return result;
            }
        }

        #endregion

        #region Gem Attributes

        public int SapphireGemBonus
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.SapphireGemEffect ?? 0;
                }

                return result;
            }
        }

        public int AmethystGemBonus
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.AmethystGemEffect ?? 0;
                }

                return result;
            }
        }

        public int CitrineGemBonus
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.CitrineGemEffect ?? 0;
                }

                return result;
            }
        }

        public int BerylGemBonus
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.BerylGemEffect ?? 0;
                }

                return result;
            }
        }

        public int AmberGemBonus
        {
            get
            {
                int result = 0;
                for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                {
                    result += UserPackage.GetEquipment(pos)?.AmberGemEffect ?? 0;
                }

                return result;
            }
        }
        #endregion

        #region Battle

        public override bool IsEmbedGemType(uint type)
        {
            for (ItemPosition i = ItemPosition.EquipmentBegin; i < ItemPosition.EquipmentEnd; i++)
            {
                var item = UserPackage.GetEquipment(i);
                if (item != null)
                {
                    if (item.GemType == type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool IsSimpleMagicAtk()
        {
            return Transformation != null && Transformation.MagicType != 0; 
        }

        public override int AdjustMagicDamage(int damage)
        {
            var status = QueryStatus(StatusSet.STATUS_MAGICDAMAGE);
            if (status != null)
            {
                damage = Calculations.CutTrail(0, Calculations.AdjustData(damage, status.Power));
            }

            return damage;
        }

        public override int AdjustExp(Role pTarget, int nRawExp, bool bNewbieBonusMsg = false)
        {
            if (pTarget == null)
            {
                return 0;
            }

            int exp = nRawExp;
            //exp = (int)BattleSystem.AdjustExp(exp, Level, pTarget.Level);
            //DynamicNpc npc = pTarget as DynamicNpc;
            //if (pTarget.IsMonster() || (npc != null && npc.IsGoal()))
            //{
            //    //if (this->GetTutor())
            //    //{
            //    //    CTeam* pTeam = this->GetTeam();
            //    //    if (pTeam && pTeam->IsTeamWithTutor(this->GetID(), pTarget))
            //    //    {
            //    //        nExp += nExp * WITHTUTORBONUS_PERCENT / 100;    // ºÍµ¼Ê¦Ò»ÆðÁ·¼¶£¬¶à20%¾­Ñé
            //    //    }
            //    //    else
            //    //    {
            //    //        nExp += nExp * HAVETUTORBONUS_PERCENT / 100;    // Ã»ºÍµ¼Ê¦Ò»ÆðÁ·¼¶£¬¶à10%¾­Ñé
            //    //    }
            //    //}
            //}

            IStatus status = QueryStatus(StatusSet.STATUS_TEAMEXP);
            if (status != null)
            {
                exp = Calculations.CutTrail(0, Calculations.MulDiv(exp, 100 + status.Power, 100));
            }

            status = QueryStatus(StatusSet.STATUS_ADD_EXP);
            if (status != null)
            {
                exp = Calculations.CutTrail(0, Calculations.MulDiv(exp, 100 + status.Power, 100));
            }

            status = QueryStatus(StatusSet.STATUS_ADJUST_EXP);
            if (status != null)
            {
                exp = Calculations.CutTrail(0, Calculations.AdjustData(exp, status.Power));
            }

            return exp;
        }

        public override int AdjustAttack(int nRawAtk)
        {
            int nAddAtk = 0;
            IStatus pStatus = QueryStatus(StatusSet.STATUS_DEF2ATK);
            if (pStatus != null)
            {
                nAddAtk += Calculations.CutTrail(0, Calculations.MulDiv(Defense, pStatus.Power, 100));
            }

            pStatus = QueryStatus(StatusSet.STATUS_ATTACK);
            if (pStatus != null)
            {
                nAddAtk += Calculations.CutTrail(0, Calculations.AdjustData(nRawAtk, pStatus.Power)) - nRawAtk;
            }

            pStatus = QueryStatus(StatusSet.STATUS_SUPER_ATK);
            if (pStatus != null)
            {
                nAddAtk += Calculations.CutTrail(0, Calculations.AdjustData(nRawAtk, pStatus.Power)) - nRawAtk;
            }
            /*
                pStatus	= QueryStatus(STATUS_TEAMATTACK);
                if (pStatus)
                {
                    nAddAtk += ::CutTrail(0, AdjustData(nAtk, pStatus->GetPower())) - nAtk;
            #ifdef _DEBUG
                    this->SendSysMsg(_TXTATR_NORMAL, "atack adjust STATUS_TEAMATTACK+: %d", pStatus->GetPower());
            #endif
                }
            */

            return nRawAtk + nAddAtk;
        }

        public override int AdjustDefence(int nDef)
        {
            int nAddDef = 0;
            IStatus pStatus = QueryStatus(StatusSet.STATUS_DEF2ATK);
            if (pStatus != null)
            {
                nAddDef += Calculations.CutTrail(0, Calculations.MulDiv(nDef, 100 - Math.Max(100, pStatus.Power), 100)) - nDef;
            }

            pStatus = QueryStatus(StatusSet.STATUS_DEFENCE3);
            if (pStatus == null)
            {
                pStatus = QueryStatus(StatusSet.STATUS_DEFENCE2);
            }

            if (pStatus == null)
            {
                pStatus = QueryStatus(StatusSet.STATUS_DEFENCE1);
            }

            if (pStatus != null)
            {
                nAddDef += Calculations.CutTrail(0, Calculations.AdjustData(nDef, pStatus.Power)) - nDef;
            }

            pStatus = QueryStatus(StatusSet.STATUS_SUPER_DEF);
            if (pStatus != null)
            {
                nAddDef += Calculations.CutTrail(0, Calculations.AdjustData(nDef, pStatus.Power)) - nDef;
            }

            pStatus = QueryStatus(StatusSet.STATUS_DEF2ATK);
            if (pStatus != null)
            {
                return (Calculations.CutTrail(0, Calculations.MulDiv(nDef + nAddDef, 100 - Math.Max(100, pStatus.Power), 100)));
            }

            return nDef + nAddDef;
        }

        public override int AdjustMagicAtk(int nAtk)
        {
            IStatus pStatus = QueryStatus(StatusSet.STATUS_SUPER_MATK);
            if (pStatus != null)
            {
                return Calculations.CutTrail(0, Calculations.AdjustData(nAtk, pStatus.Power));
            }

            return nAtk;
        }

        public override int AdjustMagicDef(int nDef)
        {
            int nAddDef = 0;
            IStatus pStatus = QueryStatus(StatusSet.STATUS_SUPER_MDEF);
            if (pStatus != null)
            {
                nAddDef += Calculations.CutTrail(0, Calculations.AdjustData(nDef, pStatus.Power));
            }

            pStatus = QueryStatus(StatusSet.STATUS_MAGICDEFENCE);
            if (pStatus != null)
            {
                nAddDef += Calculations.CutTrail(0, Calculations.AdjustData(nDef, pStatus.Power));
            }

            return nDef + nAddDef;
        }

        public override async Task<bool> TransferShieldAsync(bool bMagic, Role pAtker, int nDamage)
        {
            IStatus status = QueryStatus(StatusSet.STATUS_TRANSFER_SHIELD);
            if (status != null)
            {
                return false;
            }

            //int nMagicType = pStatus->GetPower();
            //vector<CMonster*> setEudemon;
            //// Ñ°ÕÒ·ûºÏÌõ¼þµÄÒÑ³öÕ÷»ÃÊÞ
            //for (int i = 0; i < this->GetEudemonAmount(); i++)
            //{
            //    CMonster* pEudemon = this->QueryEudemonByIndex(i);
            //    if (pEudemon && pEudemon->QueryMagic() && pEudemon->QueryMagic()->FindMagic(nMagicType))
            //    {
            //        setEudemon.push_back(pEudemon);
            //    }
            //}
            //if (setEudemon.empty())
            //{
            //    return false;
            //}
            //nDamage = nDamage / setEudemon.size();
            //for (int i = 0; i < setEudemon.size(); i++)
            //{
            //    int nLostLife = ::CutOverflow(nDamage, (int)setEudemon[i]->GetLife());
            //    if (nLostLife > 0)
            //    {
            //        setEudemon[i]->AddAttrib(_USERATTRIB_LIFE, -1 * nLostLife, SYNCHRO_TRUE);
            //    }
            //    setEudemon[i]->BeAttack(bMagic, pAtker, nDamage);
            //    // ÕâÀïÕâÑù´¦ÀíÔÚÁ¬ÕÐÖÐÓ¦ÓÃ»áµ¼ÖÂÏÈÉ±ËÀºó³öÁ¬ÕÐ¶¯×÷µÄbug
            //    // µÈ´ýÈ·¶¨½â¾ö·½°¸
            //    if (!setEudemon[i]->IsAlive())
            //    {
            //        pAtker->Kill(setEudemon[i], bMagic ? DIE_MAGIC : DIE_NORMAL);
            //    }
            //}

            return false;
        }

        public override async Task BeKillAsync(Role attacker)
        {
            // TODO: player die system
        }

        #endregion

        #region Multiple Exp

        private ExperienceManager.ExperienceMultiplierData experienceMultiplierData;

        public void LoadExperienceData()
        {
            experienceMultiplierData = ExperienceManager.GetExperienceMultiplierData(Identity);
        }

        public bool HasMultipleExp => !experienceMultiplierData.Equals(default) && experienceMultiplierData.EndTime > DateTime.Now;

        public float ExperienceMultiplier => experienceMultiplierData.Equals(default) ? 1f : experienceMultiplierData.ExperienceMultiplier;

        public uint RemainingExperienceSeconds
        {
            get
            {
                if (!experienceMultiplierData.IsActive)
                {
                    return 0;
                }

                return (uint)experienceMultiplierData.RemainingSeconds;
            }
        }

        public async Task<bool> SetExperienceMultiplierAsync(uint seconds, float multiplier = 2f)
        {
            if (ExperienceManager.AddExperienceMultiplierData(Identity, multiplier, (int)seconds))
            {
                experienceMultiplierData = ExperienceManager.GetExperienceMultiplierData(Identity);
            }
            await SendMultipleExpAsync();
            return true;
        }

        public async Task SendMultipleExpAsync()
        {
            int expSeconds = (int)RemainingExperienceSeconds;
            if (RemainingExperienceSeconds > 0)
            {
                await AttachStatusAsync(StatusSet.STATUS_ADD_EXP, 0, expSeconds, 0);
            }
        }

        #endregion

        #region Experience

        public bool IsNewbie() => Level < 70;

        public async Task<bool> AwardLevelAsync(ushort amount)
        {
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return false;
            }

            if (Level + amount <= 0)
            {
                return false;
            }

            int addLev = amount;
            if (addLev + Level > ExperienceManager.GetLevelLimit())
            {
                addLev = ExperienceManager.GetLevelLimit() - Level;
            }

            if (addLev <= 0)
            {
                return false;
            }

            await AddAttributesAsync(ClientUpdateType.Level, addLev);
            if (AutoAllot)
            {
                DbPointAllot allot = PointAllotRepository.Get(ProfessionSort, Level);
                if (allot != null)
                {
                    await SetAttributesAsync(ClientUpdateType.Force, allot.Force);
                    await SetAttributesAsync(ClientUpdateType.Speed, allot.Dexterity);
                    await SetAttributesAsync(ClientUpdateType.Health, allot.Health);
                    await SetAttributesAsync(ClientUpdateType.Spirit, allot.Soul);
                }
            }

            uint maxLife = MaxLife;
            uint maxMana = MaxMana;
            await SetAttributesAsync(ClientUpdateType.MaxLife, maxLife);
            await SetAttributesAsync(ClientUpdateType.Life, maxLife);
            await SetAttributesAsync(ClientUpdateType.Mana, maxMana);
            await SetAttributesAsync(ClientUpdateType.MaxMana, maxMana);
            await BroadcastRoomMsgAsync(new MsgAction
            {
                Identity = Identity,
                Action = EOActionType.actionUplev,
                CommandX = Level
            }, true);

            await UpLevelEventAsync();
            return true;
        }

        public async Task AwardBattleExpAsync(long experience, bool bGemEffect)
        {
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return;
            }

            if (experience == 0 || QueryStatus(StatusSet.STATUS_CANNOTSPEAK) != null)
            {
                return;
            }

            if (experience < 0)
            {
                await AddAttributesAsync(ClientUpdateType.Experience, experience);
                return;
            }

            if (Level >= 120)
            {
                experience /= 2;
            }

            double multiplier = 1;
            if (HasMultipleExp)
            {
                multiplier += ExperienceMultiplier - 1;
            }

            if (!IsNewbie() && ProfessionSort == 13 && ProfessionLevel >= 3)
            {
                multiplier += 1;
            }

            DbLevelExperience levExp = ExperienceManager.GetUserLevelXp(Level);
            //if (IsBlessed)
            //{
            //    if (levExp != null)
            //    {
            //        OnlineTrainingExp += (uint)(levExp.UpLevTime * (experience / (float)levExp.Exp));
            //    }
            //}

            if (bGemEffect)
            {
                multiplier += SapphireGemBonus / 100d;
            }

            multiplier += 1 + BattlePower / 100d;
            experience = (long)(experience * Math.Max(0.01d, multiplier));
            await AwardExperienceAsync(experience);
        }

        public long AdjustExperience(Role pTarget, long nRawExp, bool bNewbieBonusMsg)
        {
            if (pTarget == null)
            {
                return 0;
            }

            long nExp = nRawExp;
            //nExp = Battle.BattleSystem.AdjustExp((int)nExp, Level, pTarget.Level);
            return nExp;
        }

        public async Task<bool> AwardExpPercentAsync(long amount, bool noContribute = false)
        {
            if (Level > ExperienceManager.GetLevelLimit())
            {
                return true;
            }

            if (Map != null && Map.IsNoExpMap())
            {
                return false;
            }

            ulong totalLevelXp = ExperienceManager.GetUserLevelXp(Level).Exp;
            totalLevelXp = (ulong)(totalLevelXp * (amount / 10000.0f));

#if DEBUG
            logger.Information($"User {Name} received {amount/100.0f}% of exp, total of: {totalLevelXp}");
#endif

            return await AwardExperienceAsync((long)totalLevelXp, noContribute);
        }

        public async Task<bool> AwardExperienceAsync(long amount, bool noContribute = false)
        {
            if (Level > ExperienceManager.GetLevelLimit())
            {
                return true;
            }

            if (Map != null && Map.IsNoExpMap())
            {
                return false;
            }

            amount += (long)Experience;
            var leveled = false;
            byte newLevel = Level;
            double mentorUpLevTime = 0;
            while (newLevel < ExperienceManager.GetLevelLimit() && amount >= (long)ExperienceManager.GetUserLevelXp(newLevel).Exp)
            {
                DbLevelExperience dbExp = ExperienceManager.GetUserLevelXp(newLevel);
                amount -= (long)dbExp.Exp;
                leveled = true;
                newLevel++;
                mentorUpLevTime += dbExp.MentorUpLevTime;
                if (newLevel < ExperienceManager.GetLevelLimit())
                {
                    continue;
                }

                amount = 0;
                break;
            }

            if (leveled)
            {
                Level = newLevel;
                if (AutoAllot)
                {
                    DbPointAllot allot = PointAllotRepository.Get(ProfessionSort, Level);
                    if (allot != null)
                    {
                        await SetAttributesAsync(ClientUpdateType.Force, allot.Force);
                        await SetAttributesAsync(ClientUpdateType.Speed, allot.Dexterity);
                        await SetAttributesAsync(ClientUpdateType.Health, allot.Health);
                        await SetAttributesAsync(ClientUpdateType.Spirit, allot.Soul);
                    }
                }

                uint maxLife = MaxLife;
                uint maxMana = MaxMana;
                await SetAttributesAsync(ClientUpdateType.Level, Level);
                await SetAttributesAsync(ClientUpdateType.MaxLife, maxLife);
                await SetAttributesAsync(ClientUpdateType.Life, maxLife);
                await SetAttributesAsync(ClientUpdateType.Mana, maxMana);
                await SetAttributesAsync(ClientUpdateType.MaxMana, maxMana);
                await Screen.BroadcastRoomMsgAsync(new MsgAction
                {
                    Action = EOActionType.actionUplev,
                    Identity = Identity,
                    CommandX = Level
                });

                await UpLevelEventAsync();

//                if (!noContribute && Guide.Tutor != null && mentorUpLevTime > 0)
//                {
//                    mentorUpLevTime /= 5;
//                    await Guide.Tutor.AwardTutorExperienceAsync((uint)mentorUpLevTime).ConfigureAwait(false);
//#if DEBUG
//                    if (Guide.Tutor.Guide?.IsPm() == true)
//                    {
//                        await Guide.Tutor.Guide.SendAsync($"Mentor uplev time add: +{mentorUpLevTime}", TalkChannel.Talk);
//                    }
//#endif
//                }
            }

            await SetAttributesAsync(ClientUpdateType.Experience, (ulong)amount);
            return true;
        }

        public async Task UpLevelEventAsync()
        {
            await GameAction.ExecuteActionAsync(USER_UPLEV_ACTION, this, this, null, string.Empty);

            //if (Team != null)
            //{
            //    await Team.BroadcastMemberLifeAsync(this, true);
            //    await Team.SyncFamilyBattlePowerAsync();
            //}

            //if (Guide != null && Guide.ApprenticeCount > 0)
            //{
            //    await Guide.SynchroApprenticesSharedBattlePowerAsync();
            //}
        }

        public long CalculateExpBall(int amount = EXPBALL_AMOUNT)
        {
            long exp = 0;
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return 0;
            }

            byte newLevel = Level;
            if (Experience > 0)
            {
                var dbLevelXp = ExperienceManager.GetUserLevelXp(newLevel);
                if (dbLevelXp == null)
                {
                    return 0;
                }
                double pct = 1.00 - Experience / (double)dbLevelXp.Exp;
                if (amount > pct * dbLevelXp.UpLevTime)
                {
                    amount -= (int)(pct * dbLevelXp.UpLevTime);
                    exp += (long)(dbLevelXp.Exp - Experience);
                    newLevel++;
                }
            }

            var levelXp = ExperienceManager.GetUserLevelXp(newLevel);
            while (newLevel < ExperienceManager.GetLevelLimit() && amount > levelXp.UpLevTime)
            {
                amount -= levelXp.UpLevTime;
                exp += (long)levelXp.Exp;
                newLevel++;

                if (newLevel >= ExperienceManager.GetLevelLimit())
                {
                    return exp;
                }

                levelXp = ExperienceManager.GetUserLevelXp(newLevel);
            }

            if (amount > 0)
            {
                var tgtLevelXp = ExperienceManager.GetUserLevelXp(newLevel);
                if (tgtLevelXp != null)
                {
                    exp += (long)(amount / (double)tgtLevelXp.UpLevTime * tgtLevelXp.Exp);
                }
            }

            return exp;
        }

        public ExperiencePreview PreviewExpBallUsage(int amount = EXPBALL_AMOUNT)
        {   
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return new ExperiencePreview() { Level = Level };
            }

            long expBallExp = (long)Experience + CalculateExpBall(amount);
            byte newLevel = Level;
            DbLevelExperience dbExp = ExperienceManager.GetUserLevelXp(newLevel);
            while (newLevel < ExperienceManager.GetLevelLimit() && dbExp != null && expBallExp >= (long)dbExp.Exp)
            {
                expBallExp -= (long)dbExp.Exp;
                newLevel++;
                dbExp = ExperienceManager.GetUserLevelXp(newLevel);
                if (newLevel < ExperienceManager.GetLevelLimit())
                {
                    continue;
                }

                dbExp = null;
                expBallExp = 0;
                break;
            }

            double percent = 0;
            if (expBallExp > 0 && dbExp != null)
            {
                percent = (expBallExp / (double)dbExp.Exp) * 100.0f;
            }

            return new ExperiencePreview
            {
                Level = newLevel,
                Experience = (ulong)expBallExp,
                Percent = percent
            };
        }

        public ExperiencePreview PreviewExperienceIncrement(ulong experience)
        {
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return new ExperiencePreview
                {
                    Level = Level
                };
            }

            byte newLevel = Level;
            ulong newExperience = Experience + experience;
            double percent = 0;
            DbLevelExperience dbExp = ExperienceManager.GetUserLevelXp(newLevel);
            do
            {
                if (newExperience < dbExp.Exp)
                {
                    break;
                }

                newLevel++;
                newExperience -= dbExp.Exp;
                dbExp = ExperienceManager.GetUserLevelXp(newLevel);
            }
            while (newLevel < ExperienceManager.GetLevelLimit() && dbExp != null);

            if (newExperience != 0 && dbExp != null)
            {
                percent = (double)newExperience / dbExp.Exp * 100;
            }

            return new ExperiencePreview
            {
                Level = newLevel,
                Experience = newExperience,
                Percent = percent
            };
        }

        public byte UsedExpBallsAmount => (byte)(user.ExpBallUsage % 10);

        public Task IncrementExpBallAsync()
        {
            if (user.ExpBallUsage == 0)
            {
                user.ExpBallUsage = (uint)((DateTime.Now.Month * 100) + DateTime.Now.Day) * 10;
                return SaveAsync();
            }

            if (user.ExpBallUsage % 10 >= 9)
            {
                logger.Warning($"[{user.Identity}, {user.Name}] Error during expball usage, can`t exceed 10!");
                return Task.CompletedTask;
            }

            user.ExpBallUsage++;
            return SaveAsync();
        }

        public bool CanUseExpBall()
        {
            if (Level >= ExperienceManager.GetLevelLimit())
            {
                return false;
            }

            int date = (DateTime.Now.Month * 100) + DateTime.Now.Day;
            if (user.ExpBallUsage != 0 && user.ExpBallUsage / 10 != date)
            {
                user.ExpBallUsage = 0;
                return true;
            }

            if (user.ExpBallUsage % 10 >= 9)
            {
                return false;
            }

            return true;
        }

        public Task IncrementExpCrystalAsync()
        {
            if (user.ExpCrystalUsage == 0)
            {
                user.ExpCrystalUsage = (ushort)(((DateTime.Now.Month * 100) + DateTime.Now.Day) * 10);
                return SaveAsync();
            }

            if (user.ExpCrystalUsage % 10 >= 9)
            {
                logger.Warning($"[{user.Identity}, {user.Name}] Error during expcrystal usage, can`t exceed 5!");
                return Task.CompletedTask;
            }

            user.ExpCrystalUsage++;
            return SaveAsync();
        }

        public bool CanUseEudemonCrystal()
        {
            int date = (DateTime.Now.Month * 100) + DateTime.Now.Day;
            if (user.ExpCrystalUsage != 0 && user.ExpCrystalUsage / 10 != date)
            {
                user.ExpCrystalUsage = 0;
                return true;
            }

            if (user.ExpCrystalUsage % 10 >= 4)
            {
                return false;
            }

            return true;
        }

        public struct ExperiencePreview
        {
            public int Level { get; set; }
            public ulong Experience { get; set; }
            public double Percent { get; set; }
        }

        #endregion

        #region Transformation

        private TimeOut transformationTimer = new();

        public Transformation Transformation { get; protected set; }

        public async Task<bool> TransformAsync(uint dwLook, int nKeepSecs, bool bSynchro)
        {
            var bBack = false;

            if (Transformation != null)
            {
                await ClearTransformationAsync();
                bBack = true;
            }

            DbMonstertype pType = RoleManager.GetMonstertype(dwLook);
            if (pType == null)
            {
                return false;
            }

            var pTransform = new Transformation(this);
            if (pTransform.Create(pType))
            {
                Transformation = pTransform;
                TransformationMesh = (ushort)pTransform.Lookface;
                await SetAttributesAsync(ClientUpdateType.Mesh, Mesh);
                Life = MaxLife;
                transformationTimer = new TimeOut(nKeepSecs);
                transformationTimer.Startup(nKeepSecs);
                if (bSynchro)
                {
                    await SynchroTransformAsync();
                }
            }
            else
            {
                pTransform = null;
            }

            if (bBack)
            {
                await SynchroTransformAsync();
            }

            return true;
        }

        public async Task ClearTransformationAsync()
        {
            TransformationMesh = 0;
            Transformation = null;
            transformationTimer.Clear();

            await SynchroTransformAsync();
            //await MagicData.AbortMagicAsync(true);
            //BattleSystem.ResetBattle();
        }

        public async Task<bool> SynchroTransformAsync()
        {
            await SynchroAttributesAsync(ClientUpdateType.Mesh, Mesh, true);
            if (TransformationMesh != 98 && TransformationMesh != 99 && IsAlive)
            {
                Life = MaxLife;
                await SynchroAttributesAsync(ClientUpdateType.MaxLife, MaxLife);
                await SynchroAttributesAsync(ClientUpdateType.Life, Life, false);
            }
            return true;
        }

        public async Task SetGhostAsync()
        {
            if (IsAlive)
            {
                return;
            }

            ushort trans = 98;
            if (Gender == 2)
            {
                trans = 99;
            }

            TransformationMesh = trans;
            await SynchroTransformAsync();
        }

        #endregion

        #region Revive

        private readonly TimeOut reviveTimer = new();
        private readonly TimeOut protectionTimer = new();
        private bool reviveLeaveMap;

        public bool CanRevive()
        {
            return !IsAlive && reviveTimer.IsTimeOut();
        }

        public async Task RebornAsync(bool chgMap, bool isSpell = false)
        {
            if (IsAlive || !CanRevive() && !isSpell)
            {
                if (QueryStatus(StatusSet.STATUS_GHOST) != null)
                {
                    await DetachStatusAsync(StatusSet.STATUS_GHOST);
                }

                if (QueryStatus(StatusSet.STATUS_DIE) != null)
                {
                    await DetachStatusAsync(StatusSet.STATUS_DIE);
                }

                if (TransformationMesh == 98 || TransformationMesh == 99)
                {
                    await ClearTransformationAsync();
                }

                return;
            }

            reviveTimer.Clear();
            //BattleSystem.ResetBattle();

            await ClearTransformationAsync();
            await DetachStatusAsync(StatusSet.STATUS_GHOST);
            await DetachStatusAsync(StatusSet.STATUS_DIE);

            await SetAttributesAsync(ClientUpdateType.Energy, DEFAULT_USER_ENERGY);
            await SetAttributesAsync(ClientUpdateType.Life, MaxLife);
            await SetAttributesAsync(ClientUpdateType.Mana, MaxMana);
            await SetXpAsync(0);

            reviveLeaveMap = true;
            
            if (chgMap || !isSpell)
            {
                reviveLeaveMap = false;
                await FlyMapAsync(user.MapId, user.X, user.Y);
            }
            else
            {
                if (!isSpell && (Map.IsPrisionMap()
                                 || Map.IsPkField()
                                 || Map.IsCallNewbieDisable()
                || Map.IsSynMap()))
                {
                    await FlyMapAsync(user.MapId, user.X, user.Y);
                }
                else
                {
                    await FlyMapAsync(idMap, currentX, currentY);
                }
            }

            protectionTimer.Startup(CHGMAP_LOCK_SECS);
        }

        #endregion
    }
}
