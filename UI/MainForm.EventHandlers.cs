using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using AutoAssemblyMatcher.Models;

namespace AutoAssemblyMatcher.UI
{
    /// <summary>
    /// Partial class containing all event handler methods wired up from the Designer.
    /// </summary>
    public partial class MainForm : Form
    {
        private void buttonSkip_Click(object sender, EventArgs e)
        {
            NextAssemblyType();
        }

        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            ignored.Add(assemblyTypes[currentAssemblyIndex].Name);
            _persistence.SaveIgnored(ignored);

            NextAssemblyType();
        }

        private void buttonAssociate_Click(object sender, EventArgs e)
        {
            var oldType = assemblyTypes[currentAssemblyIndex].Definition;
            var newType = currentDummyTypes[currentDummyIndex].Type.Definition;

            var remap = _matching.CreateRemapEntry(oldType, newType);

            remapped.Add(oldType.Name, remap);
            _persistence.SaveRemapped(remapped);

            usedNames.Add(newType.FullName);

            NextAssemblyType();
        }

        private void comboBoxAssembly_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAssemblyTypeIndex(comboBoxAssembly.SelectedIndex);
        }

        private void scintillaAssembly_ZoomChanged(object sender, EventArgs e)
        {
            settings.Zoom = scintillaAssembly.Zoom;
            scintillaDummy.Zoom = scintillaAssembly.Zoom;
            _persistence.SaveSettings(settings);
        }

        private void scintillaDummy_ZoomChanged(object sender, EventArgs e)
        {
            settings.Zoom = scintillaDummy.Zoom;
            scintillaAssembly.Zoom = scintillaDummy.Zoom;
            _persistence.SaveSettings(settings);
        }

        private void genericScintilla_KeyDown(object sender, KeyEventArgs e)
        {
            FindReplace findReplace = null;
            if (sender == scintillaAssembly)
            {
                findReplace = _assemblyFindReplace;
            }
            else if (sender == scintillaDummy)
            {
                findReplace = _dummyFindReplace;
            }

            if (findReplace == null)
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.F)
            {
                findReplace.ShowIncrementalSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.Shift && e.KeyCode == Keys.F3)
            {
                findReplace.Window.FindPrevious();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                findReplace.Window.FindNext();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.H)
            {
                findReplace.ShowReplace();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                findReplace.ShowIncrementalSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                GoTo MyGoTo = new GoTo((Scintilla)sender);
                MyGoTo.ShowGoToDialog();
                e.SuppressKeyPress = true;
            }
        }

        private void genericFindReplace_KeyDown(object sender, KeyEventArgs e)
        {
            genericScintilla_KeyDown(sender, e);
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            settings.WindowWidth = DeviceToLogicalUnits(this.Width);
            settings.WindowHeight = DeviceToLogicalUnits(this.Height);
            _persistence.SaveSettings(settings);
        }

        private void listViewDummy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!settingDummyIndex && listViewDummy.SelectedIndices.Count > 0)
            {
                SetDummyTypeIndex(listViewDummy.SelectedIndices[0]);
            }
        }

        private void listViewDummy_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!e.Item.Selected)
            {
                e.DrawBackground();
                e.DrawText();
            }
            else
            {
                using (SolidBrush backBrush = new SolidBrush(e.Item.BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);

                    using (Pen selectionPen = new Pen(Color.Black, 3))
                    {
                        Rectangle rect = e.Bounds;
                        rect.Inflate(-2, -2);
                        e.Graphics.DrawRectangle(selectionPen, rect);
                    }
                }
                e.DrawText();
            }
        }
    }
}
