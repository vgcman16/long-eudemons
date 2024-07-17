using Long.Database.Entities;

namespace Long.Kernel.States.Items.Drops
{
    public class DropRule
    {
        private readonly List<uint> items = new();
        private readonly DbDropItemRule rule;

        public DropRule(DbDropItemRule rule)
        {
            this.rule = rule;
        }

        public uint RuleId => rule.RuleId;

        public int RuleChance => rule.Chance;

        public bool IsValid() => RuleChance > 0 && items.Count > 0;

        public bool LoadRule() 
        {
            if (rule == null)
                return false;

            if (rule.Item0 != 0)
                items.Add(rule.Item0);
            if (rule.Item1 != 0)
                items.Add(rule.Item1);
            if (rule.Item2 != 0)
                items.Add(rule.Item2);
            if (rule.Item3 != 0)
                items.Add(rule.Item3);
            if (rule.Item4 != 0)
                items.Add(rule.Item4);
            if (rule.Item5 != 0)
                items.Add(rule.Item5);
            if (rule.Item6 != 0)
                items.Add(rule.Item6);
            if (rule.Item7 != 0)
                items.Add(rule.Item7);
            if (rule.Item8 != 0)
                items.Add(rule.Item8);
            if (rule.Item9 != 0)
                items.Add(rule.Item8);
            if (rule.Item10 != 0)
                items.Add(rule.Item10);
            if (rule.Item11 != 0)
                items.Add(rule.Item11);
            if (rule.Item12 != 0)
                items.Add(rule.Item12);
            if (rule.Item13 != 0)
                items.Add(rule.Item13);
            if (rule.Item14 != 0)
                items.Add(rule.Item14);

            return true;
        }

        public uint GetDropItem(int totalChance, int rand)
        {
            uint typeId = 0;
            if (RuleChance > 0)
            {
                if (rand < totalChance + RuleChance)
                {
                    typeId = items[NextAsync(items.Count).GetAwaiter().GetResult()];
                }
            }

            return typeId;
        }
    }
}
