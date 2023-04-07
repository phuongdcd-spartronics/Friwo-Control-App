using FriwoControl.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FriwoControl
{
    /// <summary>
    /// Interaction logic for AssemblySetupWindow.xaml
    /// </summary>
    public partial class AssemblySetupWindow : Window
    {
        private readonly FriwoControlContext _dbContext = new FriwoControlContext();

        public ObservableCollection<ProdAssembly> AssemblyList { get; set; }
        public ObservableCollection<ProdProcess> ProcessList { get; set; }
        public ObservableCollection<ProdProcess> ProcessGroup { get; set; }

        public AssemblySetupWindow()
        {
            InitializeComponent();

            AssemblyList = new ObservableCollection<ProdAssembly>(_dbContext.ProdAssemblies.AsNoTracking());
            ProcessList = new ObservableCollection<ProdProcess>();
            ProcessGroup = new ObservableCollection<ProdProcess>();
            AssemblyList.Add(new ProdAssembly() { Id = 0, Name = "New..." });

            this.DataContext = this;
        }

        private void cbbAssembly_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProdAssembly selectedAssembly = cbbAssembly.SelectedValue as ProdAssembly;
            if (selectedAssembly != null)
            {
                // Insert new assembly option
                if (selectedAssembly.Id == 0)
                {
                    cbbAssembly.SelectedValue = null;
                    AssemblyInputWindow window = new AssemblyInputWindow();
                    if (window.ShowDialog() == true && window.ProdAssembly != null)
                    {
                        ProdAssembly newAssembly = window.ProdAssembly;
                        _dbContext.ProdAssemblies.Add(newAssembly);
                        if (_dbContext.SaveChanges() > 0)
                        {
                            AssemblyList.Insert(cbbAssembly.Items.Count - 1, newAssembly);
                            cbbAssembly.SelectedValue = newAssembly;
                        }
                    }
                }
                // Load process list based on selected assembly
                else
                {
                    lblQtyPer.Content = selectedAssembly.QtyPer.ToString("#,#");

                    var processes = _dbContext.ProdProcesses.ToList();
                    var assemblyInProcess = _dbContext.AssemblyInProcesses.Where(x => x.AssemblyId == selectedAssembly.Id).ToList();
                    ProcessList.Clear();
                    ProcessGroup.Clear();
                    foreach (var process in processes)
                    {
                        if (assemblyInProcess.Any(x => x.ProcessId == process.Id))
                        {
                            ProcessGroup.Add(process);
                        }
                        else
                        {
                            ProcessList.Add(process);
                        }
                    }
                }
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ProdAssembly assembly = cbbAssembly.SelectedValue as ProdAssembly;
            if (assembly != null)
            {
                // Prevent delete assembly has constraint on it
                if (_dbContext.ProductionOrders.Any(p => p.AssemblyName == assembly.Name))
                {
                    CstMessageBox.Show("Forbidden", $"Some WO were created with this assembly. Cannot delete {assembly.Name}", CstMessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show($"Removed {assembly.Name}?", "Remove", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _dbContext.AssemblyInProcesses.RemoveRange(_dbContext.AssemblyInProcesses.Where(a => a.AssemblyId == assembly.Id));
                    _dbContext.ProdAssemblies.Remove(assembly);
                    if (_dbContext.SaveChanges() > 0)
                    {
                        AssemblyList.Remove(assembly);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var assembly = cbbAssembly.SelectedValue as ProdAssembly;
            if (assembly == null || assembly.Id == 0)
            {
                CstMessageBox.Show("Missing assembly", "Please select an assembly to add processes!", CstMessageBoxIcon.Warning);
                return;
            }
            else if (ProcessGroup.Count == 0)
            {
                CstMessageBox.Show("Invalid process", "No process selected for this assembly!", CstMessageBoxIcon.Warning);
                return;
            }

            _dbContext.AssemblyInProcesses.RemoveRange(_dbContext.AssemblyInProcesses.Where(x => x.AssemblyId == assembly.Id));
            _dbContext.SaveChanges();
            int order = 1;
            foreach (ProdProcess process in ProcessGroup)
            {
                _dbContext.AssemblyInProcesses.Add(new AssemblyInProcess()
                {
                    AssemblyId = assembly.Id,
                    ProcessId = process.Id,
                    OrderNo = order++
                });
                _dbContext.SaveChanges();
            }

            CstMessageBox.Show("Success", $"Saved successfully for `{assembly.Name}`!", CstMessageBoxIcon.Success);
        }
    }
}
