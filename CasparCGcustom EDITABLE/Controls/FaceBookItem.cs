using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FotbollsVMKlocka.Controls
{
    

    public partial class FaceBookItem : UserControl
    {



        public FaceBookItem()
        {
            InitializeComponent();
        }

        public int ItemIndex { get; set; }  

        public void SetLabel(string s)
        {
            this.labelNum.Text = s;
        }

        public string FBName
        {
            get { return this.tbName.Text; }
            set { this.tbName.Text = value; }
        }

        public string FBComment
        {
            get { return this.tbComment.Text; }
            set { this.tbComment.Text = value; }
        }

        public Boolean IsChecked
        {
            get { return this.cbInclude.Checked; }
        }

    }
}
