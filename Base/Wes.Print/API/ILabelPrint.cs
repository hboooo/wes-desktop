using System.Collections.Generic;

namespace Wes.Print
{
    public interface ILabelPrint
    {
        PrintTemplateModel TemplateModel { get; set; }

        PrintParam Param { get; set; }

        void VerifyingData();

        void PreparePrinter();

        bool Print();

        void Dispose();
    }
}
