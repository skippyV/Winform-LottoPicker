
namespace NumberBoardWinFormDynamic
{
    public class LottoNumberSet
    {
        
        public static int MaxNumberValsStandard = 5;
        public static int MaxNumberValsSpecial = 1;
        public static int MaxNumberValsTotal = MaxNumberValsStandard + MaxNumberValsSpecial;

        private List<(int value, bool IsValid)> nums = new();

        public LottoNumberSet()
        {
            InitializeNumbers();
        }

        private void InitializeNumbers()
        {
            for (int i = 0; i < MaxNumberValsTotal; i++)
            {
                nums.Add((-1, false));
            }
        }

        public bool IsFull()
        {
            return IsFullStandardVals() && IsFullSpecialVals();
        }

        public bool IsFullStandardVals()
        {
            return !nums.GetRange(0, MaxNumberValsStandard).Any(m => m.IsValid == false);
        }

        public bool IsFullSpecialVals()
        {
            return !nums.GetRange(MaxNumberValsStandard, MaxNumberValsTotal - MaxNumberValsStandard).Any(m => m.IsValid == false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> -1 if no slots available, otherwise index of next available slot</returns>
        public int GetNextAvailableIndexStandardVals()
        {
            if(IsFullStandardVals())
            {
                return -1;
            }
            return nums.GetRange(0, MaxNumberValsStandard).FindIndex(m => !m.IsValid);
        }

        public int GetNextAvailableIndexSpecialVals()
        {
            if (IsFullSpecialVals())
            {
                return -1;
            }
            return nums.GetRange(MaxNumberValsStandard, MaxNumberValsSpecial).FindIndex(m => !m.IsValid) + MaxNumberValsStandard;
        }

        public bool SetValueAt(int index, int val)
        {
            if (index > MaxNumberValsTotal - 1 || index > nums.Count - 1)
            {
                return false;
            }
            (int value, bool IsValid) t = nums[index];
            t.value = val;
            t.IsValid = true;
            nums[index] = t;

            return true;
        }

        public bool RemoveElementAt(int index)
        {
            if (index > MaxNumberValsTotal - 1 || index > nums.Count - 1)
            {
                return false;
            }

            (int, bool) t = (-1, false);
            nums[index] = t;

            return true;
        }
       
    }
}
