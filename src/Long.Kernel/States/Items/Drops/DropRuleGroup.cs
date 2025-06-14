using Long.Kernel.Database.Repositories;
using Long.Kernel.Service;

namespace Long.Kernel.States.Items.Drops
{
    public class DropRuleGroup
    {
        public const int DROP_CHANCE_PRECISION = 10000000;

        private readonly List<DropRule> rules = new();
        private readonly uint groupId = 0;
        private bool isValid = true;

        public DropRuleGroup(uint groupId)
        {
            this.groupId = groupId;
        }

        public uint GroupId => groupId;
        public bool IsValid() => isValid;

        public async Task<bool> LoadGroupAsync()
        {
            if (GroupId == 0)
            {
                return false;
            }

            foreach (var rule in await DropItemRuleRepository.GetRulesByGroupAsync(groupId))
            {
                bool sameChance = false;
                var chance = rule.Chance;
                var otherRule = rules.FirstOrDefault(x=>x.RuleChance == chance);
                if (otherRule != null)
                {
                    sameChance = true;
                }

                if (!sameChance)
                {
                    var newRule = new DropRule(rule);
                    if (newRule.LoadRule())
                    {
                        rules.Add(newRule);
                    }
                }
            }

            // set group rule validation
            foreach (var rule in rules)
            {
                if (!rule.IsValid())
                {
                    isValid = false;
                    break;
                }
            }

            return IsValid();
        }

        public uint GetDropItem()
        {
            uint typeId = 0;
            int rand = Services.Randomness.NextInteger(0, DROP_CHANCE_PRECISION);
            int totalChance = 0;
            foreach (var rule in rules)
            {
                typeId = rule.GetDropItem(totalChance, rand);
                if (typeId != 0)
                {
                    break;
                }

                totalChance += rule.RuleChance;
            }

            return typeId;
        }
    }
}
