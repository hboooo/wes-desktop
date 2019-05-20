using System;
using System.Collections.Generic;

namespace Wes.Print
{
    public class PrintEventArgs : EventArgs
    {
        public PrintEventArgs(List<PrintTemplateModel> templates)
        {
            this.Templates = templates;
        }

        public PrintEventArgs(PrintTemplateModel template)
        {
            this.Template = template;
        }

        public PrintEventArgs(PrintTemplateModel template, PrintParam param) : this(template)
        {
            this.Param = param;
        }

        public PrintEventArgs(List<PrintTemplateModel> templates, PrintParam param) : this(templates)
        {
            this.Param = param;
        }


        public PrintParam Param { get; set; }

        public List<PrintTemplateModel> Templates { get; set; }

        public PrintTemplateModel Template { get; set; }

    }
}
