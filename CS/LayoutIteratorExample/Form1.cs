﻿using DevExpress.XtraRichEdit.API.Layout;
using DevExpress.XtraRichEdit.API.Native;
using System;
using System.Windows.Forms;

namespace LayoutIteratorExample
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        LayoutIterator layoutIterator;
        SubDocument doc;
        DocumentRange coloredRange;

        public Form1()
        {
            InitializeComponent();
            rgLevel.EditValueChanged += rgLevel_EditValueChanged;

            repositoryItemLayoutLevel.Items.AddRange(Enum.GetValues(typeof(LayoutLevel)));
            cmbLayoutLevel.EditValue = LayoutLevel.Box;
            cmbLayoutLevel.Enabled = false;

            richEditControl1.LoadDocument("Test.docx");
            richEditControl1.DocumentLoaded += richEditControl1_DocumentLoaded;
            doc = richEditControl1.Document;
        }

        void richEditControl1_DocumentLoaded(object sender, EventArgs e)
        {
            layoutIterator = new LayoutIterator(richEditControl1.DocumentLayout);
        }

        private void btnMoveNext_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region #MoveNext
            bool result = false;
            string s = string.Empty;

            // Create a new iterator if the document has been changed and the layout is updated.
            if (!layoutIterator.IsLayoutValid) CreateNewIterator();

            switch (barEditItemRgLevel.EditValue.ToString())
            {
                case "Any":
                    result = layoutIterator.MoveNext();
                    break;
                case "Level":
                    result = layoutIterator.MoveNext((LayoutLevel)cmbLayoutLevel.EditValue);
                    break;
                case "LevelWithinParent":
                    result = layoutIterator.MoveNext((LayoutLevel)cmbLayoutLevel.EditValue, false);
                    break;                
            }

            if (!result)
            {
                s = "Cannot move.";
                if (layoutIterator.IsStart) s += "\nStart is reached";
                else if (layoutIterator.IsEnd) s += "\nEnd is reached";
                MessageBox.Show(s);
            }
            #endregion #MoveNext
            UpdateInfoAndSelection();            
        }

        
        private void btnMovePrev_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region #MovePrev
            bool result = false;
            string s = string.Empty;
            // Create a new iterator if the document has been changed and the layout is updated.
            if (!layoutIterator.IsLayoutValid) CreateNewIterator();

            switch (barEditItemRgLevel.EditValue.ToString())
            {
                case "Any":
                    result = layoutIterator.MovePrevious();
                    break;
                case "Level":
                    result = layoutIterator.MovePrevious((LayoutLevel)cmbLayoutLevel.EditValue);
                    break;
                case "LevelWithinParent":
                    result = layoutIterator.MovePrevious((LayoutLevel)cmbLayoutLevel.EditValue, false);
                    break;
            }

            if (!result)
            {
                s = "Cannot move.";
                if (layoutIterator.IsStart) s += "\nStart is reached.";
                else if (layoutIterator.IsEnd) s += "\nEnd is reached.";
                    MessageBox.Show(s);
            }
            #endregion #MovePrev
            UpdateInfoAndSelection();
        }

        private void btnStartOver_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (coloredRange != null) ResetRange(coloredRange);
            CreateNewIterator();
        }

        private void CreateNewIterator()
        {
            layoutIterator = new LayoutIterator(richEditControl1.DocumentLayout);
            doc = richEditControl1.Document;
            UpdateInfoAndSelection();
            MessageBox.Show("Layout is modified, creating a new iterator.");
        }
        
        private void btnStartHere_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (coloredRange != null) ResetRange(coloredRange);

            doc = richEditControl1.Document.CaretPosition.BeginUpdateDocument();
            richEditControl1.Document.ChangeActiveDocument(doc);
            layoutIterator = new LayoutIterator(richEditControl1.DocumentLayout, doc.Range);

            RangedLayoutElement el = richEditControl1.DocumentLayout.GetElement(richEditControl1.Document.CaretPosition, LayoutType.PlainTextBox);
            do
            {
                RangedLayoutElement element = layoutIterator.Current as RangedLayoutElement;
                if ((element != null) && (element.Equals(el)))
                {
                    UpdateInfoAndSelection();
                    return;
                }
            } while (layoutIterator.MoveNext());
        }

        private void btnSetRange_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (coloredRange != null) ResetRange(coloredRange);

            coloredRange = richEditControl1.Document.Selection;
            if (coloredRange.Length == 0) return;

            // Highlight selected range.
            SubDocument d = coloredRange.BeginUpdateDocument();
            CharacterProperties cp = d.BeginUpdateCharacters(coloredRange);
            cp.BackColor = System.Drawing.Color.Yellow;
            d.EndUpdateCharacters(cp);
            coloredRange.EndUpdateDocument(d);

            // Create a new iterator limited to the specified range.
            layoutIterator = new LayoutIterator(richEditControl1.DocumentLayout, coloredRange);

            doc = coloredRange.BeginUpdateDocument();
            richEditControl1.Document.ChangeActiveDocument(doc);
            coloredRange.EndUpdateDocument(doc);
            
            // Select the first element in the highlighted range.
            RangedLayoutElement el = richEditControl1.DocumentLayout.GetElement(coloredRange.Start, LayoutType.PlainTextBox);
            while (layoutIterator.MoveNext())
            {
                RangedLayoutElement element = layoutIterator.Current as RangedLayoutElement;
                if ((element != null) && (element.Equals(el)))
                {
                    UpdateInfoAndSelection();
                    return;
                }
            }
        }

        private void UpdateInfoAndSelection()
        {
            LayoutElement element = layoutIterator.Current;
            infoElement.Caption = String.Empty;
            if (element != null)
            {
                RangedLayoutElement rangedElement = element as RangedLayoutElement;
                infoElement.Caption = element.Type.ToString();
                if (rangedElement != null)
                {
                    DocumentRange r = doc.CreateRange(rangedElement.Range.Start, rangedElement.Range.Length);
                    richEditControl1.Document.ChangeActiveDocument(doc);
                    richEditControl1.Document.Selection = r;
                }
            }
        }

        void ResetRange(DocumentRange r)
        {
            SubDocument d = r.BeginUpdateDocument();
            CharacterProperties cp = d.BeginUpdateCharacters(r);
            cp.BackColor = System.Drawing.Color.White;
            d.EndUpdateCharacters(cp);
            r.EndUpdateDocument(d);
        }

        private void rgLevel_EditValueChanged(object sender, EventArgs e)
        {
            string val = ((DevExpress.XtraEditors.RadioGroup)sender).EditValue.ToString();
            if (val == "Any") cmbLayoutLevel.Enabled = false;
            else cmbLayoutLevel.Enabled = true;
        }

    }
}
