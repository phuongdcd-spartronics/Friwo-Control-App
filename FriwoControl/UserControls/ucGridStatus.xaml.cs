using FriwoControl.Models;
using FriwoControl.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace FriwoControl.UserControls
{
    /// <summary>
    /// Interaction logic for ucGridStatus.xaml
    /// </summary>
    public partial class ucGridStatus : UserControl
    {
        private FriwoControlContext _dbContext = new FriwoControlContext();

        public ucGridStatus()
        {
            InitializeComponent();
        }

        private List<SerialRouting> AddBarcode(string barcode, string station)
        {
            List<SerialRouting> routes = new List<SerialRouting>();
            string action = "";
            string workOrder = null;
            string serialNumber = null;
            string revision = null;
            string[] parts = barcode.Split('-');
            string asmblyName = parts[0];
            // Check barcode contains at least 4 parts separated by dash (-)
            if (parts.Length < 4)
            {
                throw new FormatException($"{barcode} bị sai định dạng!");
            }
            ProdAssembly assembly = _dbContext.ProdAssemblies.FirstOrDefault(a => a.Name == asmblyName);
            if (assembly == null)
            {
                throw new MissingPrimaryKeyException($"Không có sản phẩm {parts[0]} trong barcode {barcode}.");
            }
            else if (!BarcodeUtils.ValidateBarcode(barcode, assembly.Type))
            {
                throw new FormatException($"{barcode} không đúng định dạng cho sản phẩm {assembly.Name}");
            }
            else
            {
                // Germany barcode format: Assembly - Work order - Serial number - Revision
                if (assembly.Type == "Germany")
                {
                    workOrder = parts[1];
                    serialNumber = parts[2];
                    revision = parts[3];
                }
                // Vietnam barcode format: Assembly - Serial number - Work order - Revision
                else if (assembly.Type == "Vietnam")
                {
                    workOrder = parts[2];
                    serialNumber = parts[1];
                    revision = parts[3];
                }
            }

            BarcodeInfo barcodeInfo = _dbContext.BarcodeInfoes.FirstOrDefault(b => b.Barcode == barcode);
            // Create new barcode if not existed
            if (barcodeInfo == null)
            {
                action = "Insert";
                barcodeInfo = new BarcodeInfo()
                {
                    Barcode = barcode,
                    AssemblyName = assembly.Name,
                    WorkOrder = workOrder,
                    SerialNumber = serialNumber,
                    Revision = revision,
                    CreatedBy = UserSession.User.Username,
                    CreatedAt = DateTime.Now
                };
                _dbContext.BarcodeInfoes.Add(barcodeInfo);
                _dbContext.SaveChanges();
            }
            // Update timestamp if barcode is existed
            else
            {
                action = "Update";
                barcodeInfo.ModifiedBy = UserSession.User.Username;
                barcodeInfo.ModifiedAt = DateTime.Now;
            }

            var processes = _dbContext.AssemblyInProcesses
                .Where(x => x.AssemblyId == assembly.Id).OrderBy(x => x.OrderNo).ToList();
            if (!processes.Any())
            {
                throw new NoNullAllowedException($"Không có quy trình cho sản phẩm `{assembly.Name}`.");
            }
            else if (processes.First().ProdProcess.Name != station)
            {
                throw new InvalidDataException($"Không thể xử lý `{barcode}` tại trạm {station}. Cần xử lý tại trạm {processes.First().ProdProcess.Name} trước!");
            }

            // Create new routing for new barcode
            foreach (AssemblyInProcess asmProc in processes)
            {
                SerialRouting route = new SerialRouting()
                {
                    SerialNumber = barcodeInfo.Barcode,
                    Station = asmProc.ProdProcess.Name,
                    Step = asmProc.OrderNo,
                    Status = DBEnum.Status.MISSING,
                    Timespan = null
                };
                _dbContext.SerialRoutings.Add(route);
                _dbContext.SaveChanges();
                routes.Add(route);
            }

            _dbContext.Histories.Add(new History()
            {
                Reference = $"{Environment.MachineName}$ICT",
                Action = action,
                Description = $"Read {barcode} for assembly {assembly.Name}",
                CreatedBy = UserSession.User.Username,
                CreatedAt = DateTime.Now
            });

            _dbContext.SaveChanges();
            return routes;
        }

        public void UpdateQuantity(string wo, string station)
        {
            try
            {
                _dbContext = new FriwoControlContext();
                var list = _dbContext.SerialRoutings.Where(r => r.WorkOrder == wo && r.Station == station).ToList();
                txtPassQty.Text = $"PASS: {list.Count(x => x.Status == DBEnum.Status.PASS):#,0}";
                txtFailQty.Text = $"FAIL: {list.Count(x => x.Status == DBEnum.Status.FAIL):#,0}";
                txtMissQty.Text = $"MISSING: {list.Count(x => x.Status == DBEnum.Status.MISSING):#,0}";
                txtPassQty.Visibility = System.Windows.Visibility.Visible;
                txtFailQty.Visibility = System.Windows.Visibility.Visible;
                txtMissQty.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                //Logging.Error(ex, "Update quantity failed!");
            }
        }

        public bool UpdateProcess(ProductionOrder wo, string shift, string barcode, string station, string status, out SerialRouting route, bool needUpdate = true)
        {
            _dbContext = new FriwoControlContext();
            gvProcess.ItemsSource = null;

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    List<SerialRouting> routes = _dbContext.SerialRoutings.Where(r => r.SerialNumber == barcode)
                        .OrderBy(r => r.Step)
                        .ToList();
                    // Create route if not existing
                    if (!routes.Any())
                    {
                        routes = AddBarcode(barcode, station);
                    }

                    BarcodeInfo barcodeInfo = _dbContext.BarcodeInfoes.FirstOrDefault(b => b.Barcode == barcode);
                    SerialRouting current = routes.FirstOrDefault(r => r.Station == station);

                    if (wo.AssemblyName != barcodeInfo.AssemblyName)
                        throw new InvalidDataException($"`{barcode}` không thuộc sản phẩm `{wo.AssemblyName}`.");
                    else if (wo.WO != barcodeInfo.WorkOrder)
                        throw new InvalidDataException($"`{barcode}` không thuộc WO `{barcodeInfo.WorkOrder}`.");

                    if (current == null)
                        throw new NotImplementedException($"`{barcodeInfo.AssemblyName}` không có quy trình {station.ToUpper()}.");
                    else if (current.Status == DBEnum.Status.PASS)
                        throw new ReadOnlyException($"{barcode} đã được sử dụng tại trạm này.");

                    // Get previous step which status isn't passed
                    SerialRouting failedRoute = routes.FirstOrDefault(r => r.Step < current.Step && r.Status != DBEnum.Status.PASS);
                    // Exclude current step in the result if the current step does not need to update
                    int limitedStep = needUpdate ? current.Step : current.Step - 1;
                    List<SerialRouting> updatedRoutes = routes.Where(r => r.Step <= limitedStep).ToList();

                    // Update UI grid
                    gvProcess.ItemsSource = updatedRoutes;

                    if (failedRoute != null)
                    {
                        throw new InvalidOperationException($"{barcode} chưa qua trạm {failedRoute.Station}.");
                    }
                    // Update current step status when all previous steps are passed
                    else if (needUpdate)
                    {
                        current.WorkOrder = wo.WO;
                        current.AssemblyName = wo.AssemblyName;
                        current.Shift = shift;
                        current.Status = status;
                        current.Timespan = DateTime.Now;

                        // Add to history
                        _dbContext.Histories.Add(new History()
                        {
                            Action = "Update",
                            Reference = $"{Environment.MachineName}${station}",
                            Description = $"Updated {barcode} at {station}: [{status}] (WO: {wo.WO}; Shift: {shift})",
                            CreatedBy = UserSession.User.Username,
                            CreatedAt = DateTime.Now
                        });
                        _dbContext.SaveChanges();
                        //Logging.Info($"Update {barcode} with status {status} at {station}.");
                    }

                    transaction.Commit();

                    route = current;
                    return !updatedRoutes.Any(r => r.Status != DBEnum.Status.PASS);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}