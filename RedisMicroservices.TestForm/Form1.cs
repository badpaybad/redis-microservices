using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain;
using RedisMicroservices.TestForm.Business;

namespace RedisMicroservices.TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            new DistributedServices().PublishDataModel(new DistributedCommandDataModel<SampleData>(
                new SampleData()
                {
                    Id = Guid.NewGuid(),
                    Version = DateTime.Now.ToString(),
                    LanguageCode = "Vn",
                    CreatedDate = DateTime.Now
                }, EntityAction.Insert));

            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dgvList.DataSource = SampleBusiness.GetAll();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            dgvList.DataSource = SampleBusiness.GetAll();
        }
    }
}