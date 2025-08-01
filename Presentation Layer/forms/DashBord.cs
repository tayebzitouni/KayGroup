using BusinessAccessLayer;
using BussinessAcesssLayer;
using DataAccessLayer;
using Guna.UI2.WinForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
using static Dtos.Dtos;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Drawing2D;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.forms;



namespace freelanceProject1.Presentation_Layer
{





    public partial class DashBord : Form
    {

        private DateTime from = new DateTime(DateTime.Today.Year, 1, 1);
        private DateTime to = new DateTime(DateTime.Today.Year, 12, 31);

        public DashBord()
        {
            InitializeComponent();
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox1);
            guna2DataGridView11.EnableHeadersVisualStyles = false;
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView9.EnableHeadersVisualStyles = false;
            guna2DataGridView9.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView9.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView9.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            LoadFacturesClientsIntoGrid();
            LoadFacturesFournisseursIntoGrid();

            guna2ComboBox1.BackColor = Color.FromArgb(16, 35, 40); // اللون الأساسي
            guna2ComboBox1.ForeColor = Color.White;
            guna2ComboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            guna2ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2ComboBox1.FlatStyle = FlatStyle.Flat;

            guna2ComboBox1.MouseEnter += (s, e) =>
            {
                guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#F9E4A7"); // أصفر
            };

            guna2ComboBox1.MouseLeave += (s, e) =>
            {

                guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);
            };

            // عند التركيز (focus)
            guna2ComboBox1.Enter += (s, e) =>
            {
                guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#F9E4A7");
            };

            // عند فقدان التركيز
            guna2ComboBox1.Leave += (s, e) =>
            {
                guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);
            };



        }


        private async void boxes()
        {
            label10.Text = FactureClientBussinesLayer.GetTotalAmountOfNonPayeFacturesAsync().ToString() + " MAD";
            label19.Text = FactureFournisseurBusinessLayer.GetTotalNonpayeFactureFacturesFourinsseurs().ToString() + " MAD";
            decimal a = await PaymentBussiness.BankSolde();
            label35.Text = a.ToString() + " MAD";
            a = EntityBussiness.GetNumberOfEntitesNotAsync();
            label29.Text = Convert.ToInt32(a).ToString();
        }

        private ComboBox comboBoxFeature;


        private void InitFeatureChart(List<EntitySummary> data)
        {
            guna2Panel4.Controls.Clear();

            // 1. إنشاء الكومبوبوكس
            comboBoxFeature = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Width = 200,
                Location = new Point(10, 10),

            };

            comboBoxFeature.Items.AddRange(new string[] {
        "Chiffre d'affaires", "Charges", "Rsultat"
    });
            comboBoxFeature.SelectedIndex = 0;

            // 2. حدث التغيير
            comboBoxFeature.SelectedIndexChanged += (s, e) =>
            {
                DrawSingleFeatureChart(comboBoxFeature.SelectedItem.ToString(), data);
            };

            // 3. نضيفه للـ Panel
            guna2Panel4.Controls.Add(comboBoxFeature);

            // 4. أول رسم
            DrawSingleFeatureChart(comboBoxFeature.SelectedItem.ToString(), data);
        }

        private double CalculateNiceInterval(double min, double max)
        {
            double range = max - min;
            if (range <= 0) return 1; // Avoid division by 0
            double roughInterval = range / 5; // 5 steps for readability
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughInterval)));
            double normalized = roughInterval / magnitude;

            if (normalized < 1.5)
                return 1 * magnitude;
            else if (normalized < 3)
                return 2 * magnitude;
            else if (normalized < 7)
                return 5 * magnitude;
            else
                return 10 * magnitude;
        }



        //private void DrawSingleFeatureChart(string featureType, List<EntitySummary> data)
        //{
        //    for (int i = guna2Panel4.Controls.Count - 1; i >= 0; i--)
        //    {
        //        if (!(guna2Panel4.Controls[i] is ComboBox))
        //            guna2Panel4.Controls.RemoveAt(i);
        //    }

        //    var chart = new Chart
        //    {
        //        Dock = DockStyle.Fill,
        //        BackColor = Color.White,
        //        Padding = new Padding(10, 50, 10, 10)
        //    };

        //    var area = new ChartArea("Main");
        //    area.AxisX.Interval = 0.5;
        //    area.AxisX.IsMarginVisible = true;
        //    chart.ChartAreas.Add(area);
        //    chart.ChartAreas[0].AxisX.Interval = 1;
        //    area.AxisX.MajorGrid.Enabled = false;
        //    area.AxisY.MajorGrid.LineColor = Color.LightGray;
        //    area.AxisX.LabelStyle.Font = new Font("Segoe UI Semibold", 9);
        //    area.AxisY.LabelStyle.Format = "N0";

        //    double maxValue = featureType switch
        //    {
        //        "Chiffre d'affaires" => (double)data.Max(e => e.ChiffreAffaire),
        //        "Charges" => (double)data.Max(e => e.Charges),
        //        "Rsultat" => (double)data.Max(e => e.Resultat),
        //        _ => 0
        //    };

        //    double minValue = featureType switch
        //    {
        //        "Chiffre d'affaires" => (double)data.Min(e => e.ChiffreAffaire),
        //        "Charges" => (double)data.Min(e => e.Charges),
        //        "Rsultat" => (double)data.Min(e => e.Resultat),
        //        _ => 0
        //    };

        //    if (maxValue == 0 && minValue == 0)
        //    {
        //        area.AxisY.Minimum = -10;
        //        area.AxisY.Maximum = 10; // fixe
        //    }
        //    else
        //    {
        //        تأكد أن هناك دائمًا فرق بين الأدنى والأعلى(Y range)
        //        double yMin = minValue < 0 ? Math.Floor(minValue * 1.2) : 0;
        //        double yMax = maxValue > 0 ? Math.Ceiling(maxValue * 1.2) : 10;

        //        if (yMax - yMin < 1)
        //        {
        //            yMax = yMin + 10; // لإجبار المحور على عرض الفرق
        //        }

        //        area.AxisY.Minimum = yMin;
        //        area.AxisY.Maximum = yMax;
        //    }


        //    var series = new Series(featureType)
        //    {
        //        ChartType = SeriesChartType.Column,
        //        IsValueShownAsLabel = false, // ❌ don't show default labels
        //        ["PointWidth"] = "0.3"
        //    };
        //    series["PixelPointWidth"] = "50";
        //    Color labelColor;
        //    switch (featureType)
        //    {
        //        case "Chiffre d'affaires":
        //            series.Color = Color.FromArgb(0, 145, 179);
        //            labelColor = Color.Black;
        //            break;
        //        case "Charges":
        //            series.Color = Color.FromArgb(255, 191, 0);
        //            labelColor = Color.Black;
        //            break;
        //        case "Rsultat":
        //            series.Color = Color.FromArgb(0, 86, 107); // Or any custom color
        //            labelColor = Color.Black;
        //            break;
        //        default:
        //            labelColor = Color.Black;
        //            break;
        //    }
        //    int index = 0;
        //    foreach (var entity in data)
        //    {
        //        decimal value = featureType == "Chiffre d'affaires" ? entity.ChiffreAffaire
        //                          : featureType == "Charges" ? entity.Charges
        //                          : entity.Resultat;

        //        series.Points.AddXY(entity.Entity, value);

        //        double annotationY = (double)value > 0
        //            ? (double)value + Math.Abs(maxValue) * 0.05
        //            : (double)value - Math.Abs(minValue) * 0.05;

        //        var annotation = new TextAnnotation
        //        {
        //            Text = (value < 0 ? "-" : "") + Math.Abs(value).ToString("N0"),
        //            ForeColor = labelColor,
        //            Font = new Font("Segoe UI", 9, FontStyle.Bold),
        //            AnchorDataPoint = series.Points[index],
        //            AnchorY = annotationY,
        //            AnchorAlignment = (double)value > 0 ? ContentAlignment.BottomCenter : ContentAlignment.TopCenter,
        //            AxisX = area.AxisX2,
        //            AxisY = area.AxisY,
        //            BackColor = Color.Transparent
        //        };

        //        chart.Annotations.Add(annotation);
        //        index++;
        //    }

        //    chart.Series.Add(series);

        //    chart.Titles.Add(new Title($"Vue: {featureType} par entité")
        //    {
        //        Font = new Font("Segoe UI Semibold", 12),
        //        ForeColor = Color.Black
        //    });

        //    chart.Legends.Add(new Legend
        //    {
        //        Docking = Docking.Top,
        //        Alignment = StringAlignment.Center,
        //        Font = new Font("Segoe UI", 9)
        //    });

        //    guna2Panel4.Controls.Add(chart);
        //}


        private void DrawSingleFeatureChart(string featureType, List<EntitySummary> data)
        {
            for (int i = guna2Panel4.Controls.Count - 1; i >= 0; i--)
            {
                if (!(guna2Panel4.Controls[i] is ComboBox))
                    guna2Panel4.Controls.RemoveAt(i);
            }

            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 50, 10, 10)
            };

            var area = new ChartArea("Main");
            chart.ChartAreas.Add(area);

            area.AxisX.Interval = 1;
            area.AxisX.IsMarginVisible = true;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI Semibold", 9);
            area.AxisY.LabelStyle.Format = "N0";

            // ✅ Centrer l'axe OX (Y=0)
            area.AxisY.Crossing = 0;
            area.AxisX.LineColor = Color.Black;
            area.AxisX.LineWidth = 2;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;

            double maxValue = featureType switch
            {
                "Chiffre d'affaires" => (double)data.Max(e => e.ChiffreAffaire),
                "Charges" => (double)data.Max(e => e.Charges),
                "Rsultat" => (double)data.Max(e => e.Resultat),
                _ => 0
            };

            double minValue = featureType switch
            {
                "Chiffre d'affaires" => (double)data.Min(e => e.ChiffreAffaire),
                "Charges" => (double)data.Min(e => e.Charges),
                "Rsultat" => (double)data.Min(e => e.Resultat),
                _ => 0
            };

            if (maxValue == 0 && minValue == 0)
            {
                area.AxisY.Minimum = -10;
                area.AxisY.Maximum = 10;
            }
            //else
            //{
            //    double yMin = minValue < 0 ? Math.Floor(minValue * 1.2) : 0;
            //    double yMax = maxValue > 0 ? Math.Ceiling(maxValue * 1.2) : 10;

            //    if (yMax - yMin < 1)
            //        yMax = yMin + 10;

            //    area.AxisY.Minimum = yMin;
            //    area.AxisY.Maximum = yMax;
            //}

            area.AxisY.Interval = CalculateNiceInterval(area.AxisY.Minimum, area.AxisY.Maximum);

            var series = new Series(featureType)
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = false,
                ["PointWidth"] = "0.3"
            };
            series["PixelPointWidth"] = "50";

            Color labelColor;
            switch (featureType)
            {
                case "Chiffre d'affaires":
                    series.Color = Color.FromArgb(0, 145, 179);
                    labelColor = Color.Black;
                    break;
                case "Charges":
                    series.Color = Color.FromArgb(255, 191, 0);
                    labelColor = Color.Black;
                    break;
                case "Rsultat":
                    series.Color = Color.FromArgb(0, 145, 179);
                    labelColor = Color.Black;
                    break;
                default:
                    labelColor = Color.Black;
                    break;
            }

            int index = 0;
            foreach (var entity in data)
            {
                decimal value = featureType == "Chiffre d'affaires" ? entity.ChiffreAffaire
                                  : featureType == "Charges" ? entity.Charges
                                  : entity.Resultat;

                series.Points.AddXY(entity.Entity, value);


                double annotationY = (double)value > 0
                    ? (double)value + Math.Abs(maxValue) * 0.05
                    : (double)value + Math.Abs(minValue) * 0.05;

                var annotation = new TextAnnotation
                {
                    Text = value.ToString("N0"), // Affiche le "-" automatiquement si négatif
                    ForeColor = labelColor,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    AnchorDataPoint = series.Points[index],
                    AnchorY = annotationY,
                    AnchorAlignment = (double)value > 0 ? ContentAlignment.BottomCenter : ContentAlignment.TopCenter,
                    AxisX = area.AxisX,
                    AxisY = area.AxisY,
                    BackColor = Color.Transparent
                };

                chart.Annotations.Add(annotation);
                index++;
            }

            chart.Series.Add(series);

            chart.Titles.Add(new Title($"Vue: {featureType} par entité")
            {
                Font = new Font("Segoe UI Semibold", 12),
                ForeColor = Color.Black
            });

            chart.Legends.Add(new Legend
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9)
            });

            guna2Panel4.Controls.Add(chart);
        }



        private void isi()
        {
            decimal a = FactureClientBussinesLayer.GetTotalAmountOfAllFacturesAsync(from, to);
            decimal b = FactureFournisseurBusinessLayer.GetTotalAmountOfAllFacturesAsync(from, to);
            decimal c = a - b;
            c = c * SettingsService.GetIsRate();
            if (c > 0)
                label89.Text = c.ToString() + " MAD";
            else
            {
                label89.Text = " 0 MAD";
            }
        }

        private void Tva()
        {
            decimal tvacollect = FactureClientBussinesLayer.GetTotalTVACollectedInCurrentTrimester(from, to);
            decimal tvadeductible = FactureFournisseurBusinessLayer.GetTotalTVACollectedInCurrentTrimester(from, to);
            decimal reeturn = FactureFournisseurBusinessLayer.GetTotalTVAReturnCollectedInCurrentTrimester(from, to);
            label79.Text = tvacollect.ToString() + " MAD";
            label77.Text = tvadeductible.ToString() + " MAD";
            label91.Text = (tvacollect - tvadeductible - reeturn).ToString() + " MAD";
            usefulFunction.UsefulFuncitonClass.TextColor(tvacollect - tvadeductible - reeturn, label91);


            try
            {
                guna2ProgressBar2.Value = (int)((tvadeductible / tvacollect) * 100);
            }
            catch (Exception)
            {
                guna2ProgressBar2.Value = 0;
            }
            label93.Text = " " + guna2ProgressBar2.Value.ToString() + " %";
        }



        private async Task LoadFacturesClientsIntoGrid()
        {


            try
            {
                guna2DataGridView11.Rows.Clear();
                var users = await BussinessAcesssLayer.FactureClientBussinesLayer.GetLast10Async();




                foreach (var user in users)
                {
                    if (user.Status == "Non payé")
                    {
                        if (DateTime.Parse(user.DateEcheance) < DateTime.Today)
                        {
                            user.Status = "En retard";
                        }
                    }
                    guna2DataGridView11.Rows.Add(
                    "FC-" + user.id,
                    user.clientname.ToString(),
                    user.entiteName.ToString(),
                    user.DateEmission,
                    user.DateEcheance,
                    user.MontantTH,
                    user.TVa,
                    user.Total,
                    user.Status);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async Task LoadFacturesFournisseursIntoGrid()
        {
            try
            {
                guna2DataGridView9.Rows.Clear();
                var users = await FactureFournisseurBusinessLayer.GetLast10Async();

                foreach (var user in users)
                {
                    if (user.Status == "Non payé")
                    {
                        if (DateTime.Parse(user.DateEcheance) < DateTime.Today)
                        {
                            user.Status = "En retard";
                        }
                    }
                    guna2DataGridView9.Rows.Add(
                    "FF-" + user.id,
                    user.fournisseurname.ToString(),
                    user.entiteName.ToString(),
                    user.DateReception,
                    user.DateEcheance,
                    user.MontantTH,
                    user.TVa,
                    user.Retenue,
                    user.Total,
                    user.Status);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async void DashBord_Load(object sender, EventArgs e)
        {
            label25.Text = SettingsService.GetName();
            label26.Text = BussinessAcesssLayer.UtilisatuerBussiness.getLogInUtilsatuer().Email;
            label27.Text = BussinessAcesssLayer.UtilisatuerBussiness.getLogInUtilsatuer().Name;

            boxes();
            Tva();
            isi();
            guna2DateTimePicker4.Value = from;
            guna2DateTimePicker3.Value = to;
            using var context = new AppDbContext();

            //var groupedData = await context.payments
            //    .GroupBy(p => p.entity)
            //    .Select(g => new
            //    {
            //        Entity = g.Key,
            //        ChiffreAffaire = g.Where(p => p.Type == "Incomes").Sum(p => p.Amount),
            //        Charges = g.Where(p => p.Type == "Outcomes").Sum(p => p.Amount),
            //    })
            //    .ToListAsync();
            var allPayments = await context.payments
    .Include(p => p.entity)
    .ToListAsync();


            var groupedData = allPayments
                .GroupBy(p => p.entity)
                .Select(g => new
                {
                    Entity = g.Key,

                    ChiffreAffaire = g.Sum(p =>
                    {
                        var disc = p.GetType().Name;
                        if (disc == "PaymentParClient")
                        {
                            var child = (PaymentParClient)p;
                            return child.devis != "MAD"
                                ? (child.Amount) * (child.rate)
                                : (child.Amount);
                        }
                        else if (disc == "PaymentUtilisatuer")
                        {
                            var child = (PaymentUtilisatuer)p;
                            return child.devis != "MAD"
                                ? (child.debit) * (child.rate)
                                : (child.debit);
                        }
                        else
                        {
                            return p.Type == "Incomes" ? p.Amount : 0; // أي نوع آخر، فقط المداخيل
                        }
                    }),


                    Charges = g
                        .Where(p => p.Type == "Outcomes")
                        .Sum(p =>
                        {
                            if (p is PaymentFournisseur child)
                            {
                                return child.devis != "MAD"
                                    ? (child.Amount) * (child.rate)
                                    : (child.Amount);
                            }
                            else if (p is PaymentUtilisatuer child2)
                            {
                                return child2.devis != "MAD"
                                    ? (child2.Amount) * (child2.rate)
                                    : (child2.Amount);
                            }
                            else
                            {
                                return p.Amount;
                            }
                        })
                })
                .ToList();


            var finalData = groupedData.Select(g => new EntitySummary
            {
                Entity = g.Entity.Name,
                ChiffreAffaire = g.ChiffreAffaire,
                Charges = g.Charges,
                Resultat = g.ChiffreAffaire - g.Charges // ✅ Add this line
            }).ToList();



            InitFeatureChart(finalData);

            foreach (Control ctrl in guna2Panel3.Controls)
            {
                if (ctrl is Guna2GroupBox gb)
                {
                    gb.MouseEnter += GroupBox_MouseEnter;
                    gb.MouseLeave += GroupBox_MouseLeave;
                    gb.Click += GroupBox_Click;
                }
            }

            guna2Panel1.Focus();
        }









        private void guna2CirclePictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2ContainerControl1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox8_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2DataGridView11_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {

        }

        private void guna2DataGridView11_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 8);
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
        }



        private void guna2DataGridView9_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        public void LoadFormIntoPanel(Form formToLoad)
        {
            guna2Panel1.Controls.Clear();
            // Clear the previous form
            formToLoad.TopLevel = false; // This is needed to add a form inside a panel
            formToLoad.FormBorderStyle = FormBorderStyle.None;
            // Hide title bar
            formToLoad.Dock = DockStyle.Fill; // Make it fill the panel
            guna2Panel1.Controls.Add(formToLoad); // Add it to the panel
            formToLoad.Show();
        }

        private void guna2Panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2DateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            from = guna2DateTimePicker4.Value;
            Tva();
            isi();
        }

        private void guna2DateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            to = guna2DateTimePicker3.Value;
            Tva();
            isi();
        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2GroupBox23_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private Guna2GroupBox selectedGroupBox = null;

        private void guna2GroupBox13_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new Fiscal());
        }

        private void guna2GroupBox1_MouseEnter(object sender, EventArgs e)
        {
            enter(sender);

        }


        private void click(object sender)
        {
            var gb = sender as Guna2GroupBox;

            // Deselect previous
            if (selectedGroupBox != null)
                selectedGroupBox.FillColor = Color.Transparent;

            // Select new one
            selectedGroupBox = gb;
            selectedGroupBox.FillColor = ColorTranslator.FromHtml("#0091B3");
            guna2Panel1.Focus(); /// Blue
        }


        private void enter(object sender)
        {
            var gb = sender as Guna2GroupBox;

            if (gb != selectedGroupBox)
                gb.FillColor = ColorTranslator.FromHtml("#102328");
        }


        private void sortir(object sender)
        {
            var gb = sender as Guna2GroupBox;


            if (gb != selectedGroupBox)
                gb.FillColor = Color.Transparent;
        }




        private void GroupBox_MouseEnter(object sender, EventArgs e)
        {
            enter(sender);
        }

        private void GroupBox_MouseLeave(object sender, EventArgs e)
        {
            sortir(sender);
        }

        private void GroupBox_Click(object sender, EventArgs e)
        {
            click(sender);
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new DashBord());
        }

        private void guna2GroupBox5_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Fournisseur());
        }

        private void guna2GroupBox7_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.CLientsForm());
        }

        private void guna2GroupBox2_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Factures());
        }

        private void guna2GroupBox11_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.IncomesPayment());
        }

        private void guna2GroupBox4_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Payment());
        }

        private void guna2GroupBox8_Click_1(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Trésorerie());
        }

        private void guna2GroupBox13_Click_1(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Fiscal());
        }

        private void guna2GroupBox15_Click(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.hi());
        }

        private void guna2DataGridView11_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2DataGridView11_CellPainting_1(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 8);
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
        }

        private void guna2DataGridView9_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 8);
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
        }

        private void guna2Panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2ComboBox1_Enter(object sender, EventArgs e)
        {
            // guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#F9E4A7");
        }

        private void guna2ComboBox1_Leave(object sender, EventArgs e)
        {
            //guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#102328");
        }

        private void guna2ComboBox1_MouseEnter(object sender, EventArgs e)
        {
            // guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#F9E4A7 ");
        }

        private void guna2ComboBox1_MouseLeave(object sender, EventArgs e)
        {
            // guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);
            //MessageBox.Show("hi");
            //// guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);
        }

        private void guna2ComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            guna2ComboBox1.FillColor = Color.FromArgb(16, 35, 40);

        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            new LogIn();

            UtilisatuerBussiness.setloginutilisateru(null);
            this.Close();
        }

        private void guna2GroupBox23_Click_1(object sender, EventArgs e)
        {
            LoadFormIntoPanel(new forms.Paramètres());
        }

        private void guna2DataGridView11_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}  
