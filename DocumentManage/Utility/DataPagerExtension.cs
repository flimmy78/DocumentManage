using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace DocumentManage.Utility
{
    public static class DataPagerExtension
    {
        public static void BindSource(this DataPager dataPager, int totalCount, int pageSize)
        {
            var list = new List<int>(totalCount);
            for (int i = 0; i < totalCount; i++) list.Add(i);
            var pcv = new PagedCollectionView(list);
            pcv.PageSize = pageSize;
            dataPager.Source = pcv;
        }
    }
}
