using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using SmartDesign.DrawingDataGenerator;

namespace SmartDesign.DrawingDataGenerator
{
    public partial class DrawingDataGeneratorForm : Form
    {
        XDocument xDocument = new XDocument();

        public DrawingDataGeneratorForm()
        {
            InitializeComponent();
        }


        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false; //중복금지

            openFileDialog.Filter = "Visio 파일 (*.vsdx)|*.vsdx"; //확장자 선택
            openFileDialog.Title = "파일 가져오기";

            DialogResult dr = openFileDialog.ShowDialog();

            if (dr == DialogResult.OK)
            {
                string fileFullName = openFileDialog.FileName;

                VisioReader visioReader = new VisioReader(fileFullName);
                xDocument = visioReader.GenerateXML();
            }
            //취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
            else if (dr == DialogResult.Cancel)
            {
            }

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "파일 저장하기";
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "Xml 파일(*.xml)|*.xml";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                xDocument.Save(saveFileDialog.FileName);
            }
        }
    }
}
