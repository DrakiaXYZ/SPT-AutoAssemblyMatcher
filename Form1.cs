using AsmResolver.DotNet;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using NaturalSort.Extension;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

// Please forgive me for what you're about to read

namespace AutoAssemblyMatcher
{
    public partial class Form1 : Form
    {
        private List<AssemblyType> assemblyTypes;
        private List<(AssemblyType Type, double Score)> currentDummyTypes = new();
        private List<string> usedNames = new();
        private List<string> mappedNames = new();

        private List<string> ignored = new();
        private Dictionary<string, Remap> remapped = new();

        private ModuleDefinition assemblyDefinition;
        private ModuleDefinition dummyDefinition;
        CSharpDecompiler assemblyDecompiler;
        CSharpDecompiler dummyDecompiler;

        private Settings? settings = null;

        private int currentAssemblyIndex = 0;
        private int currentDummyIndex = 0;

        private string settingsPath = Settings.GetFilePath("settings.json");
        private string remappedPath = Settings.GetFilePath("remapped.json");
        private string ignoredPath = Settings.GetFilePath("ignored.json");

        public Form1()
        {
            InitializeComponent();

            AISlopStyler styler = new AISlopStyler();
            styler.ApplyScintillaStyle(scintilla);
            styler.ApplyScintillaStyle(scintilla1);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadSettings();

            if (settings == null)
            {
                MessageBox.Show("Error loading or storing settings. Exiting");
                Environment.Exit(0);
                return;
            }

            scintilla.Zoom = settings.Zoom;
            scintilla1.Zoom = settings.Zoom;
            this.Width = LogicalToDeviceUnits(settings.WindowWidth);
            this.Height = LogicalToDeviceUnits(settings.WindowHeight);

            LoadMappings();
            LoadIgnored();
            LoadRemapped();
            assemblyDefinition = ModuleDefinition.FromFile(settings.AssemblyDll);
            dummyDefinition = ModuleDefinition.FromFile(settings.DummyDll);

            assemblyDecompiler = CreateDecompiler(settings.AssemblyDll);
            dummyDecompiler = CreateDecompiler(settings.DummyDll);

            // DEBUG
            //GetAssemblySource(dummyDecompiler, "GoToSuppressionFireRequest");
            //GetAssemblySource(dummyDecompiler, "EFT.AreaStageSerializer");

            // Load classes from assemblyDefinition that start with "GClass"
            assemblyTypes = assemblyDefinition
                .TopLevelTypes.Where(t =>
                    (t.Methods.Count > 0 || t.Fields.Count > 0 || t.Properties.Count > 0) &&
                    !remapped.Keys.Contains(t.Name) &&
                    !mappedNames.Contains(t.Name) &&
                    !ignored.Contains(t.Name) &&
                    t.Name.ToString().StartsWith("GClass")
                // DEBUG
                //&& t.Name.ToString().StartsWith("GClass557")
                )
                .Select(t =>
                {
                    return new AssemblyType(t.Name, t);
                })
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
                .ToList();

            // DEBUG
            //var dummy = dummyDefinition.TopLevelTypes.FirstOrDefault(x => x.Name.ToString().StartsWith("PatrolPointChooserBoss"));
            //AssemblyComparator.Calculate(assemblyTypes[0].Definition, dummy);
            // DEBUG

            comboBoxAssembly.Items.Clear();
            comboBoxAssembly.Items.AddRange(assemblyTypes.Select(t => t.Name).ToArray());
            comboBoxAssembly.SelectedIndex = 0;

            SetAssemblyTypeIndex(0);
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsPath))
            {
                try
                {
                    var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                    settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath), options);
                }
                catch (Exception) { }
            }

            if (settings == null)
            {
                string assemblyDll = SelectFile("Deobfuscated Assembly", false);
                string dummyDll = SelectFile("Dummy Assembly", false);
                string mappingFile = SelectFile("GClass Mappings JSON", true);

                if (!string.IsNullOrEmpty(assemblyDll) && !string.IsNullOrEmpty(dummyDll) && !string.IsNullOrEmpty(mappingFile))
                {
                    settings = new Settings(assemblyDll, dummyDll, mappingFile);
                    SaveSettings();
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

        private void LoadMappings()
        {
            var options = new JsonDocumentOptions { AllowTrailingCommas = true };

            var mappings = JsonNode.Parse(File.ReadAllText(settings.MappingFile), documentOptions: options).AsObject();
            foreach (var property in mappings)
            {
                usedNames.Add(property.Value["NewName"].ToString());
                mappedNames.Add(property.Key);
            }
        }

        private void LoadIgnored()
        {
            if (File.Exists(ignoredPath))
            {
                var json = File.ReadAllText(ignoredPath);
                try
                {
                    var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                    ignored = JsonSerializer.Deserialize<List<string>>(json, options);
                }
                catch (Exception ex)
                {
                    if (json.Length > 0)
                    {
                        MessageBox.Show($"Exception loading ignored.json, verify or delete file. Exiting\n\n{ex.Message}");
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void LoadRemapped()
        {
            if (File.Exists(remappedPath))
            {
                var json = File.ReadAllText(remappedPath);
                try
                {
                    var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                    remapped = JsonSerializer.Deserialize<Dictionary<string, Remap>>(json, options);
                }
                catch (Exception ex)
                {
                    if (json.Length > 0)
                    {
                        MessageBox.Show($"Exception loading remapped.json, verify or delete file. Exiting\n\n{ex.Message}");
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void SaveIgnored()
        {
            SaveToJson(ignoredPath, ignored);
        }

        private void SaveRemapped()
        {
            SaveToJson(remappedPath, remapped);
        }

        private void SaveSettings()
        {
            SaveToJson(settingsPath, settings);
        }

        private void SaveToJson<T>(string path, T obj)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, IndentSize = 4, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string settingsJson = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(path, settingsJson);
        }

        private void SetAssemblyTypeIndex(int index)
        {
            currentAssemblyIndex = index;
            comboBoxAssembly.SelectedIndex = index;

            AssemblyType type = assemblyTypes[index];

            scintilla.ReadOnly = false;
            string code = GetAssemblySource(assemblyDecompiler, type.Definition.FullName);
            scintilla.Text = code;
            scintilla.ScrollWidth = 1;
            scintilla.ReadOnly = true;

            // Get list of matching dummy classes
            currentDummyTypes = AssemblyComparator.Calculate(type.Definition, dummyDefinition)
                .Where(x => !usedNames.Contains(x.Type.Name))
                .Select(x =>
                {
                    return (new AssemblyType(x.Type.Name, x.Type), x.Score);
                }).ToList();

            comboBoxDummy.Items.Clear();
            comboBoxDummy.Items.AddRange(currentDummyTypes.Select(t => $"{t.Type.Name} ({Math.Floor(t.Score)}%)").ToArray());
            if (comboBoxDummy.Items.Count > 0)
            {
                comboBoxDummy.SelectedIndex = 0;
            }

            if (currentDummyTypes.Where(t => t.Score == 100).Count() > 1)
            {
                comboBoxDummy.BackColor = Color.Salmon;
            }
            else
            {
                comboBoxDummy.BackColor = SystemColors.Window;
            }

            SetDummyTypeIndex(0);
            buttonAssociate.Enabled = currentDummyTypes.Count > 0;
        }

        private void SetDummyTypeIndex(int index)
        {
            if (index < currentDummyTypes.Count)
            {
                currentDummyIndex = index;
                comboBoxDummy.SelectedIndex = index;

                var (type, score) = currentDummyTypes[index];

                // Set the textbox
                scintilla1.ReadOnly = false;
                string code = GetAssemblySource(dummyDecompiler, type.Definition.FullName);
                scintilla1.Text = code;
                scintilla1.ReadOnly = true;
            }
            else
            {
                scintilla1.ReadOnly = false;
                scintilla1.Text = "N/A";
                scintilla1.ReadOnly = true;
            }
            scintilla1.ScrollWidth = 1;

            buttonPrev.Enabled = index > 0;
            buttonNext.Enabled = currentDummyTypes.Count > index + 1;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            SetDummyTypeIndex(currentDummyIndex + 1);
        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {
            SetDummyTypeIndex(currentDummyIndex - 1);
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            NextAssemblyType();
        }

        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            ignored.Add(assemblyTypes[currentAssemblyIndex].Name);
            SaveIgnored();

            NextAssemblyType();
        }

        private void buttonAssociate_Click(object sender, EventArgs e)
        {
            var oldType = assemblyTypes[currentAssemblyIndex].Definition;
            var newType = currentDummyTypes[currentDummyIndex].Type.Definition;

            var remap = new Remap(newType.Name);
            if (newType.Namespace is not null)
            {
                remap.NewNamespace = newType.Namespace;
            }
            if (newType.NestedTypes.Count > 0)
            {
                remap.HasChildClasses = oldType.NestedTypes.Any(x => !x.IsCompilerGenerated());
            }

            remapped.Add(oldType.Name, remap);
            SaveRemapped();

            usedNames.Add(newType.Name);

            NextAssemblyType();
        }

        private void comboBoxAssembly_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAssemblyTypeIndex(comboBoxAssembly.SelectedIndex);
        }

        private void comboBoxDummy_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDummyTypeIndex(comboBoxDummy.SelectedIndex);
        }

        private void scintilla_ZoomChanged(object sender, EventArgs e)
        {
            settings.Zoom = scintilla.Zoom;
            scintilla1.Zoom = scintilla.Zoom;
            SaveSettings();
        }

        private void scintilla1_ZoomChanged(object sender, EventArgs e)
        {
            settings.Zoom = scintilla1.Zoom;
            scintilla.Zoom = scintilla1.Zoom;
            SaveSettings();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            settings.WindowWidth = DeviceToLogicalUnits(this.Width);
            settings.WindowHeight = DeviceToLogicalUnits(this.Height);
            SaveSettings();
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

        private CSharpDecompiler CreateDecompiler(string assemblyPath)
        {
            var settings = new DecompilerSettings()
            {
                OptionalArguments = false,
                NamedArguments = false,
                SortCustomAttributes = false,
                UseExpressionBodyForCalculatedGetterOnlyProperties = false,
            };

            return new CSharpDecompiler(assemblyPath, settings);
        }

        private string GetAssemblySource(CSharpDecompiler decompiler, string fullClassName)
        {
            FullTypeName typeName = new FullTypeName(fullClassName);
            SyntaxTree syntaxTree = decompiler.DecompileType(typeName);

            foreach (var node in syntaxTree.Descendants.OfType<EntityDeclaration>())
            {
                node.Attributes.Clear();
            }

            int GetMemberPriority(EntityDeclaration member) => member switch
            {
                PropertyDeclaration => 1,
                ConstructorDeclaration => 2,
                MethodDeclaration => 3,
                FieldDeclaration => 4,
                TypeDeclaration => 5,
                _ => 6
            };

            var allTypes = syntaxTree.Descendants
                .OfType<TypeDeclaration>()
                .ToList();

            foreach (var type in allTypes)
            {
                var sortedMembers = type.Members.OrderBy(GetMemberPriority).ToList();

                type.Members.Clear();
                foreach (var member in sortedMembers)
                {
                    type.Members.Add(member);
                }
            }

            var settings = FormattingOptionsFactory.CreateSharpDevelop();
            using (var writer = new StringWriter())
            {
                var visitor = new CSharpOutputVisitor(writer, settings);
                syntaxTree.AcceptVisitor(visitor);
                return writer.ToString();
            }
        }

        private void Finished()
        {
            scintilla.ReadOnly = scintilla1.ReadOnly = false;
            scintilla.Text = scintilla1.Text = "Finished";
            scintilla.ReadOnly = scintilla1.ReadOnly = true;

            buttonSkip.Enabled = false;
            buttonIgnore.Enabled = false;
            buttonAssociate.Enabled = false;
            buttonNext.Enabled = false;
            buttonPrev.Enabled = false;
        }

        private class AssemblyType
        {
            public string Name { get; set; }
            public TypeDefinition Definition { get; set; }

            public AssemblyType(string name, TypeDefinition definition)
            {
                Name = name;
                Definition = definition;
            }
        }

        private class Remap
        {
            public Remap(string newName)
            {
                NewName = newName;
            }

            public string NewName { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? NewNamespace { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool HasChildClasses { get; set; } = false;
        }

        private class Settings
        {
            public Settings(string assemblyDll, string dummyDll, string mappingFile)
            {
                AssemblyDll = assemblyDll;
                DummyDll = dummyDll;
                MappingFile = mappingFile;
            }

            public string AssemblyDll { get; set; }
            public string DummyDll { get; set; }
            public string MappingFile { get; set; }
            public int Zoom { get; set; } = 0;
            public int WindowWidth { get; set; } = 1200;
            public int WindowHeight { get; set; } = 800;

            public static string GetFilePath(string filename)
            {
                string baseDirectory = AppContext.BaseDirectory;
                return Path.Combine(baseDirectory, filename);
            }
        }
    }
}
