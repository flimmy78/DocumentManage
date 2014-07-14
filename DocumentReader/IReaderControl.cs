using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentReader
{
    public interface IReaderControl
    {
        string FileName { get; set; }

        void Open();

        void Close();

    }
}
