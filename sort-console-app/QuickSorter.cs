using System.Collections.Generic;

class QuickSorter
{
    public static void Sort(List<string> list)
    {
        QuickSort(list, 0, list.Count - 1);
    }

    private static void QuickSort(List<string> list, int left, int right)
    {
        if (left >= right) return;

        string pivot = list[(left + right) / 2];
        int index = Partition(list, left, right, pivot);
        QuickSort(list, left, index - 1);
        QuickSort(list, index, right);
    }

    private static int Partition(List<string> list, int left, int right, string pivot)
    {
        while (left <= right)
        {
            while (Utils.CompareLines(list[left], pivot) < 0) left++;
            while (Utils.CompareLines(list[right], pivot) > 0) right--;

            if (left <= right)
            {
                (list[left], list[right]) = (list[right], list[left]);
                left++;
                right--;
            }
        }
        return left;
    }
}
