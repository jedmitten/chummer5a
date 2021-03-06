/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public sealed partial class frmSelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly CharacterOptions _objOptions;
        private readonly bool _blnUseCurrentValues = false;
        int intQualityLimits = 0;
        decimal decNuyenBP = 0;

        #region Control Events
        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter;
            _objOptions = _objCharacter.Options;
            _blnUseCurrentValues = blnUseCurrentValues;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            // Populate the Build Method list.
            List<ListItem> lstBuildMethod = new List<ListItem>
            {
                new ListItem("Karma", LanguageManager.GetString("String_Karma", GlobalOptions.Language)),
                new ListItem("Priority", LanguageManager.GetString("String_Priority", GlobalOptions.Language)),
                new ListItem("SumtoTen", LanguageManager.GetString("String_SumtoTen", GlobalOptions.Language)),
            };

            if (GlobalOptions.LifeModuleEnabled)
            {
                lstBuildMethod.Add(new ListItem("LifeModule", LanguageManager.GetString("String_LifeModule", GlobalOptions.Language)));
            }

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.SelectedValue = _objOptions.BuildMethod;
            cboBuildMethod.EndUpdate();

            nudKarma.Value = _objOptions.BuildPoints;
            nudMaxAvail.Value = _objOptions.Availability;

            // Populate the Gameplay Options list.
            string strDefault = string.Empty;
            XmlDocument objXmlDocumentGameplayOptions = XmlManager.Load("gameplayoptions.xml");
            XmlNodeList objXmlGameplayOptionList = objXmlDocumentGameplayOptions.SelectNodes("/chummer/gameplayoptions/gameplayoption");

            List<ListItem> lstGameplayOptions = new List<ListItem>();
            foreach (XmlNode objXmlGameplayOption in objXmlGameplayOptionList)
            {
                string strName = objXmlGameplayOption["name"].InnerText;
                if (objXmlGameplayOption["default"]?.InnerText == "yes")
                    strDefault = strName;
                lstGameplayOptions.Add(new ListItem(strName, objXmlGameplayOption["translate"]?.InnerText ?? strName));
            }

            cboGamePlay.BeginUpdate();
            cboGamePlay.ValueMember = "Value";
            cboGamePlay.DisplayMember = "Name";
            cboGamePlay.DataSource = lstGameplayOptions;
            cboGamePlay.SelectedValue = strDefault;
            cboGamePlay.EndUpdate();

            toolTip1.SetToolTip(chkIgnoreRules, LanguageManager.GetString("Tip_SelectKarma_IgnoreRules", GlobalOptions.Language));

            if (blnUseCurrentValues)
            {
                cboGamePlay.SelectedValue = _objCharacter.GameplayOption;

                cboBuildMethod.Enabled = false;
                cboBuildMethod.SelectedValue = Enum.GetName(Type.GetType(nameof(CharacterBuildMethod)), _objCharacter.BuildMethod);

                nudKarma.Value = objCharacter.BuildKarma;
                nudMaxNuyen.Value = decNuyenBP = _objCharacter.NuyenMaximumBP;

                intQualityLimits = _objCharacter.GameplayOptionQualityLimit;
                chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                nudMaxAvail.Value = objCharacter.MaximumAvailability;
                nudSumtoTen.Value = objCharacter.SumtoTen;
            }
            else
            {
                XmlNode objXmlSelectedGameplayOption = objXmlDocumentGameplayOptions.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + cboGamePlay.Text + "\"]");
                intQualityLimits = Convert.ToInt32(objXmlSelectedGameplayOption["karma"].InnerText);
                decNuyenBP = Convert.ToDecimal(objXmlSelectedGameplayOption["maxnuyen"].InnerText, GlobalOptions.InvariantCultureInfo);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            switch (cboBuildMethod.SelectedValue.ToString())
            {
                case "Karma":
                    _objCharacter.NuyenMaximumBP = decimal.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.Karma;
                    break;
                case "Priority":
                    _objCharacter.NuyenMaximumBP = decNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.Priority;
                    break;
                case "SumtoTen":
                    _objCharacter.NuyenMaximumBP = decNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.SumtoTen;
                    _objCharacter.SumtoTen = decimal.ToInt32(nudSumtoTen.Value);
                    break;
                case "LifeModule":
                    _objCharacter.NuyenMaximumBP = decimal.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.LifeModule;
                    break;
            }
            _objCharacter.BuildPoints = 0;
            _objCharacter.BuildKarma = decimal.ToInt32(nudKarma.Value);
            _objCharacter.GameplayOption = cboGamePlay.SelectedValue.ToString();
            _objCharacter.GameplayOptionQualityLimit = intQualityLimits;
            _objCharacter.IgnoreRules = chkIgnoreRules.Checked;
            _objCharacter.MaximumAvailability = decimal.ToInt32(nudMaxAvail.Value);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBuildMethod.SelectedValue == null)
            {
                lblDescription.Text = LanguageManager.GetString("String_SelectBP_PrioritySummary", GlobalOptions.Language);
                nudKarma.Visible = false;
            }
            else
            {
                if (cboBuildMethod.SelectedValue.ToString() == "Karma")
                {
                    if (_objOptions.BuildMethod == "Karma")
                    {
                        nudKarma.Value = _objOptions.BuildPoints;
                    }
                    else
                    {
                        nudKarma.Value = 800;
                    }
                    lblDescription.Text = LanguageManager.GetString("String_SelectBP_KarmaSummary", GlobalOptions.Language).Replace("{0}", nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    nudMaxNuyen.Visible = true;
                    nudKarma.Visible = true;
                    nudSumtoTen.Visible = false;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "Priority")
                {
                    lblDescription.Text = LanguageManager.GetString("String_SelectBP_PrioritySummary", GlobalOptions.Language);
                    nudKarma.Visible = false;
                    nudMaxNuyen.Visible = false;
                    nudSumtoTen.Visible = false;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "SumtoTen")
                {
                    lblDescription.Text = LanguageManager.GetString("String_SelectBP_PrioritySummary", GlobalOptions.Language);
                    nudKarma.Visible = false;
                    nudMaxNuyen.Visible = false;
                    nudSumtoTen.Visible = true;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "LifeModule")
                {
                    nudKarma.Value = 750;
                    lblDescription.Text = String.Format(LanguageManager.GetString("String_SelectBP_LifeModuleSummary", GlobalOptions.Language), nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    nudKarma.Visible = true;
                    nudMaxNuyen.Visible = true;
                    nudSumtoTen.Visible = false;
                }
                lblStartingKarma.Visible = nudKarma.Visible;
                lblSumToX.Visible = nudSumtoTen.Visible;
                lblMaxNuyen.Visible = nudMaxNuyen.Visible;
            }
        }

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            Height = cmdOK.Bottom + 40;
            cboBuildMethod_SelectedIndexChanged(this, e);
        }
        #endregion

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            XmlDocument objXmlDocumentGameplayOption = XmlManager.Load("gameplayoptions.xml");
            XmlNode objXmlGameplayOption = objXmlDocumentGameplayOption.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + cboGamePlay.SelectedValue.ToString() + "\"]");
            nudMaxAvail.Value = Convert.ToInt32(objXmlGameplayOption["maxavailability"].InnerText);
            intQualityLimits = Convert.ToInt32(objXmlGameplayOption["karma"].InnerText);
            decNuyenBP = Convert.ToDecimal(objXmlGameplayOption["maxnuyen"].InnerText, GlobalOptions.InvariantCultureInfo);
        }
    }
}
