using System;
using System.Linq;
using Core.Game;
using Core.Common;
using UnityEngine;
using Inventory.Main.Item;
using Newtonsoft.Json;

namespace Inventory.Main
{
    [Serializable]
    public class Bag
    {
        [JsonProperty] [field: SerializeField]
        public int SupplementSlotCount { get; private set; } = 15;
        
        [JsonProperty] [field: SerializeField]
        public int GearSlotCount { get; private set; } = 25;
        
        [JsonProperty] [field: SerializeField]
        public float Limit { get; private set; } = 50f;
        
        [JsonIgnore] public bool Initialized { get; private set; }

        [JsonIgnore] public float Weight => GetTotalSupplementWeight() + GetTotalGearWeight();

        [JsonProperty] [field: HideInInspector] [field: SerializeReference]
        public IGear[] Gears { get; private set; }
        
        [JsonProperty] [field: HideInInspector] [field: SerializeReference]
        public ISupplement[] Supplements { get; private set; }
        
        public void Initialize()
        {
            if (Initialized) return;

            //if null initialize
            Supplements ??= new ISupplement[SupplementSlotCount];

            Gears ??= new IGear[GearSlotCount];
            
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
            Gears = Resize(slots, Gears);
        }
        
        public void ResizeSupplementSlots(int slots)
        {
            Supplements = Resize(slots, Supplements);
        }

        // resizes items collection
        private T[] Resize<T>(int newSlots, T[] arrayToResize) where T : IItem
        {
            newSlots = Mathf.Clamp(newSlots, 0, int.MaxValue);
            
            Array.Resize(ref arrayToResize, newSlots);

            return arrayToResize;
        }
        
        public void ResizeLimit(float limit)
        {
            Limit = Mathf.Clamp(limit, 0, float.MaxValue);
        }
    }
}