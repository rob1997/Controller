using System;
using System.Linq;
using Core.Game;
using Core.Utils;
using UnityEngine;
using Inventory.Main.Item;

namespace Inventory.Main
{
    [CreateAssetMenu(order = 0, fileName = nameof(Bag), menuName = GameManager.StudioPrefix + "/Inventory/Bag")]
    public class Bag : ScriptableObject
    {
        [field: SerializeField] public int SupplementSlotCount { get; private set; } = 15;
        
        [field: SerializeField] public int GearSlotCount { get; private set; } = 25;
        
        [field: SerializeField] public float Limit { get; private set; } = 50f;
        
        [field: SerializeField] public bool Initialized { get; private set; }

        public float Weight => GetTotalSupplementWeight() + GetTotalGearWeight();

        [field: SerializeReference] public IGear[] Gears { get; private set; }
        
        [field: SerializeReference] public ISupplement[] Supplements { get; private set; }
        
        public void Initialize()
        {
            if (Initialized) return;

            Supplements = new ISupplement[SupplementSlotCount];

            Gears = new IGear[GearSlotCount];

            Initialized = true;

            Debug.Log("Initialized Bag...");
        }

        public bool Add(IItem item, out string message)
        {
            switch (item)
            {
                case ISupplement supplement:
                    return AddSupplement(supplement, out message);
                case IGear gear:
                    return AddGear(gear, out message);
                default:
                    message = $"Add not Implemented for {item.GetType()}";
                    return false;
            }
        }

        private bool AddGear(IGear gear, out string message)
        {
            string title = gear.Title;

            if (IsOverweight(gear.Reference.Weight))
            {
                message = $"Can't add {title}, too heavy";

                return false;
            }

            int index = GetEmptyGearSlot();

            if (index == -1)
            {
                message = $"Can't add {title}, No empty Slots";

                return false;
            }

            //clone so we're not referencing the original object
            Gears[index] = gear.Clone<IGear>();

            message = $"{title} added successfully";

            return true;
        }

        private bool AddSupplement(ISupplement supplement, out string message)
        {
            string title = supplement.Title;

            if (IsOverweight(supplement.Reference.Weight))
            {
                message = $"Can't add {title}, too heavy";

                return false;
            }

            int index = GetSupplementSlot(supplement);

            if (index == -1)
            {
                message = $"Can't add {title}, No empty Slots";

                return false;
            }

            int count = supplement.Count;

            if (Supplements[index] != null)
            {
                ISupplement item = Supplements[index];

                int remainder = item.Remainder;

                if (remainder == 0)
                {
                    message = $"Can't add more {title}, Limit Reached";

                    return false;
                }

                //count can't be more than remaining space
                count = Mathf.Clamp(count, 0, remainder);

                item.Add(count);
            }

            else
            {
                //clone so we're not referencing the original object
                Supplements[index] = supplement.Clone<ISupplement>();
            }

            supplement.Remove(count);

            message = $"{title} added successfully";

            return true;
        }

        public void RemoveSupplement(int index)
        {
            Supplements[index] = null;
        }

        public void RemoveGear(int index)
        {
            Gears[index] = null;
        }

        private int GetEmptyGearSlot()
        {
            return Array.FindIndex(Gears, g => g == null);
        }

        private int GetSupplementSlot(ISupplement supplement)
        {
            int index = Array.FindIndex(Supplements, s => s != null && s.Reference == supplement.Reference);

            if (index == -1)
            {
                index = GetEmptySupplementSlot();
            }

            return index;
        }

        private int GetEmptySupplementSlot()
        {
            return Array.FindIndex(Supplements, s => s == null);
        }

        private float GetTotalSupplementWeight()
        {
            return Supplements.Where(s => s != null).Sum(s => s.Reference.Weight);
        }

        private float GetTotalGearWeight()
        {
            return Gears.Where(g => g != null).Sum(g => g.Reference.Weight);
        }

        private bool IsOverweight(float weight)
        {
            return Weight + weight > Limit;
        }

        public void ResizeGearSlots(int slots)
        {
            GearSlotCount = Mathf.Clamp(slots, 0, int.MaxValue);
        }
        
        public void ResizeSupplementSlots(int slots)
        {
            SupplementSlotCount = Mathf.Clamp(slots, 0, int.MaxValue);
        }
        
        public void ResizeLimit(float limit)
        {
            Limit = Mathf.Clamp(limit, 0, float.MaxValue);
        }
    }
}