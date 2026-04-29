using AsmResolver.DotNet;
using ICSharpCode.Decompiler.CSharp;
using ScintillaNET_FindReplaceDialog;
using AutoAssemblyMatcher.Models;
using AutoAssemblyMatcher.Services;

namespace AutoAssemblyMatcher.UI
{
    /// <summary>
    /// Main form — fields, constructor, initialization, navigation, and UI helpers.
    /// Event handlers are in MainForm.EventHandlers.cs (partial class).
    /// </summary>
    public partial class MainForm : Form
    {
        private List<AssemblyType> assemblyTypes;
        private List<(AssemblyType Type, double Score)> currentDummyTypes = new();
        private HashSet<string> usedNames = new();
        private HashSet<string> mappedNames = new();

        private HashSet<string> ignored = new();
        private Dictionary<string, RemapEntry> remapped = new();

        private ModuleDefinition assemblyDefinition;
        private ModuleDefinition dummyDefinition;
        CSharpDecompiler assemblyDecompiler;
        CSharpDecompiler dummyDecompiler;

        private AppSettings? settings = null;

        private int currentAssemblyIndex = 0;
        private int currentDummyIndex = 0;

        private readonly PersistenceService _persistence = new();
        private readonly DecompilationService _decompilation = new();
        private readonly MatchingService _matching = new();

        private bool settingDummyIndex = false;
        private FindReplace _assemblyFindReplace;
        private FindReplace _dummyFindReplace;

        public MainForm()
        {
            InitializeComponent();

            ScintillaDarkTheme styler = new ScintillaDarkTheme();
            styler.ApplyScintillaStyle(scintillaAssembly);
            styler.ApplyScintillaStyle(scintillaDummy);

            _assemblyFindReplace = new FindReplace(scintillaAssembly);
            _assemblyFindReplace.KeyPressed += genericFindReplace_KeyDown;
            _dummyFindReplace = new FindReplace(scintillaDummy);
            _dummyFindReplace.KeyPressed += genericFindReplace_KeyDown;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            LoadSettings();

            if (settings == null)
            {
                MessageBox.Show("Error loading or storing settings. Exiting");
                Environment.Exit(0);
                return;
            }

            scintillaAssembly.Zoom = settings.Zoom;
            scintillaDummy.Zoom = settings.Zoom;
            this.Width = LogicalToDeviceUnits(settings.WindowWidth);
            this.Height = LogicalToDeviceUnits(settings.WindowHeight);

            assemblyDefinition = ModuleDefinition.FromFile(settings.AssemblyDll);
            dummyDefinition = ModuleDefinition.FromFile(settings.DummyDll);
            assemblyDecompiler = _decompilation.CreateDecompiler(settings.AssemblyDll);
            dummyDecompiler = _decompilation.CreateDecompiler(settings.DummyDll);

            var mappingResult = _persistence.LoadMappings(settings.MappingFile);
            usedNames = mappingResult.UsedNames;
            mappedNames = mappingResult.MappedNames;

            ignored = _persistence.LoadIgnored();
            remapped = _persistence.LoadRemapped(usedNames);

            var assemblyTypeNames = assemblyDefinition.TopLevelTypes.Select(x => x.Name?.ToString() ?? "");
            remapped = _persistence.CleanRemapped(remapped, mappedNames, assemblyTypeNames);

            assemblyTypes = _matching.LoadFilteredAssemblyTypes(assemblyDefinition, remapped, mappedNames, ignored);

            // DEBUG
            //var assemblyType = assemblyTypes.FirstOrDefault(t => t.Name.ToString().StartsWith("GClass2497"))?.Definition;
            //var dummyType = dummyDefinition.TopLevelTypes.FirstOrDefault(x => x.Name.ToString().StartsWith("UnitySerializedDictionary"));
            //AssemblyComparator.Calculate(assemblyType, dummyType);
            // DEBUG

            comboBoxAssembly.Items.Clear();
            comboBoxAssembly.Items.AddRange(assemblyTypes.Select(t => t.Name).ToArray());
            comboBoxAssembly.SelectedIndex = 0;

            SetAssemblyTypeIndex(0);
        }

        private void LoadSettings()
        {
            settings = _persistence.LoadSettings();

            if (settings == null)
            {
                string assemblyDll = SelectFile("Deobfuscated Assembly", false);
                string dummyDll = SelectFile("Dummy Assembly", false);
                string mappingFile = SelectFile("GClass Mappings JSON", true);

                if (!string.IsNullOrEmpty(assemblyDll) && !string.IsNullOrEmpty(dummyDll) && !string.IsNullOrEmpty(mappingFile))
                {
                    settings = new AppSettings(assemblyDll, dummyDll, mappingFile);
                    _persistence.SaveSettings(settings);
                }
            }
        }

        private string SelectFile(string title, bool json)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = json ? $"{title} (*.json5)|*.json5" : $"{title} (Assembly-CSharp*.dll)|Assembly-CSharp*.dll";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = title;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return "";
        }

        private void SetAssemblyTypeIndex(int index)
        {
            currentAssemblyIndex = index;
            comboBoxAssembly.SelectedIndex = index;

            AssemblyType type = assemblyTypes[index];

            scintillaAssembly.ReadOnly = false;
            string code = _decompilation.DecompileType(assemblyDecompiler, type.Definition.FullName);
            scintillaAssembly.Text = code;
            scintillaAssembly.ScrollWidth = 1;
            scintillaAssembly.ReadOnly = true;

            currentDummyTypes = _matching.FindCandidateMatches(type.Definition, dummyDefinition, usedNames);

            PopulateDummyListView();

            SetDummyTypeIndex(0);
            buttonAssociate.Enabled = currentDummyTypes.Count > 0;
        }

        private void PopulateDummyListView()
        {
            listViewDummy.Items.Clear();
            var multipleHundred = currentDummyTypes.Where(t => t.Score == 100).Count() > 1;
            foreach (var dummyType in currentDummyTypes)
            {
                var listViewItem = new ListViewItem($"{dummyType.Type.Name} ({Math.Floor(dummyType.Score)}%)");
                if (dummyType.Score == 100 && multipleHundred)
                {
                    listViewItem.BackColor = Color.Salmon;
                }
                listViewDummy.Items.Add(listViewItem);

                // Limit to first N results, for sanity's sake
                if (listViewDummy.Items.Count >= AppConstants.MaxDummyResults)
                {
                    break;
                }
            }

            listViewDummy.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            if (listViewDummy.Columns[0].Width < listViewDummy.ClientSize.Width - 4)
            {
                listViewDummy.Columns[0].Width = listViewDummy.ClientSize.Width - 4;
            }
        }

        private void SetDummyTypeIndex(int index)
        {
            if (settingDummyIndex)
            {
                return;
            }

            if (index < currentDummyTypes.Count)
            {
                currentDummyIndex = index;
                if (listViewDummy.Items.Count > index)
                {
                    settingDummyIndex = true;
                    listViewDummy.Items[index].Selected = true;
                    listViewDummy.Items[index].EnsureVisible();
                    listViewDummy.Items[index].Focused = true;
                    settingDummyIndex = false;
                }

                var (type, score) = currentDummyTypes[index];

                // Set the textbox
                scintillaDummy.ReadOnly = false;
                string code = _decompilation.DecompileType(dummyDecompiler, type.Definition.FullName);
                scintillaDummy.Text = code;
                scintillaDummy.ReadOnly = true;
            }
            else
            {
                scintillaDummy.ReadOnly = false;
                scintillaDummy.Text = "N/A";
                scintillaDummy.ReadOnly = true;
            }
            scintillaDummy.ScrollWidth = 1;
        }

        private int DeviceToLogicalUnits(int deviceUnits)
        {
            float scalingFactor = this.DeviceDpi / 96f;
            return (int)Math.Round(deviceUnits / scalingFactor);
        }

        private void NextAssemblyType()
        {
            if (currentAssemblyIndex >= assemblyTypes.Count - 1)
            {
                Finished();
            }
            else
            {
                SetAssemblyTypeIndex(currentAssemblyIndex + 1);
            }
        }

        private void Finished()
        {
            scintillaAssembly.ReadOnly = scintillaDummy.ReadOnly = false;
            scintillaAssembly.Text = scintillaDummy.Text = "Finished";
            scintillaAssembly.ReadOnly = scintillaDummy.ReadOnly = true;

            buttonSkip.Enabled = false;
            buttonIgnore.Enabled = false;
            buttonAssociate.Enabled = false;
        }
    }
}
