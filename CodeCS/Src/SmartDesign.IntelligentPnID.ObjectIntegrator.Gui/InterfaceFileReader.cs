using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    class InterfaceFileReader
    {
        public InterfaceConfiguration Read(string path)
        {
            XDocument document = XDocument.Load(path);

            XElement configurationElement = document.Root;
            if (configurationElement.Name != "Configuration")
                throw new Exception("파일은 <Configuration>로 시작해야 합니다.");

            InterfaceConfiguration configuration = new InterfaceConfiguration();

            string stepStr = configurationElement.Element("Step").Value;
            configuration.CurrentStep = ParseStep(stepStr);

            configuration.Step1Result = configurationElement.Element("ResultInfo").Element("Step1Result").Attribute("path").Value;
            configuration.Step2Result = configurationElement.Element("ResultInfo").Element("Step2Result").Attribute("path").Value;

            return configuration;
        }

        private InterfaceStep ParseStep(string step)
        {
            step = step.ToUpper();

            if (step == "STEP1_START")
                return InterfaceStep.Step1Start;
            else if (step == "STEP1_EDIT")
                return InterfaceStep.Step1Edit;
            else if (step == "STEP2_START")
                return InterfaceStep.Step2Start;
            else if (step == "STEP2_EDIT")
                return InterfaceStep.Step2Edit;
            else if (step == "STEP3_START")
                return InterfaceStep.Step3Start;
            else if (step == "STEP3_EDIT")
                return InterfaceStep.Step3Edit;

            throw new ArgumentException($"파싱이 불가능한 문자열입니다:{step}");
        }
    }
}
