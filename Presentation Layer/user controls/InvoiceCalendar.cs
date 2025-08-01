using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO; // Needed for Path.Combine
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataAccessLayer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;


namespace freelanceProject1.Presentation_Layer.user_controls
{
    public partial class InvoiceCalendar : UserControl
    {
        public InvoiceCalendar()
        {
            InitializeComponent();
        }

        private void InvoiceCalendar_Load(object sender, EventArgs e)
        {
            string htmlPath = Path.Combine(Application.StartupPath, "calendar.html");
            webView21.Source = new Uri(htmlPath);
           // webView21.MouseWheel += ParentControl_MouseWheel;
            webView21.PreviewKeyDown += webView21_PreviewKeyDown;

            InitializeWebView2();

        }

        private async void InitializeWebView2()
        {
            await webView21.EnsureCoreWebView2Async();

            // ✅ Directly subscribe after initialization
            if (webView21.CoreWebView2 != null)
            {
                webView21.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                Debug.WriteLine("✅ Subscribed to WebMessageReceived");
            }
            else
            {
                MessageBox.Show("❌ CoreWebView2 is null after initialization.");
            }
        }



        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();
            Debug.WriteLine($"Received message from JS: hi hi h i");

            string dateFromJs = e.TryGetWebMessageAsString();

            List<string> factures = SearchFacturesByDate(dateFromJs);

            string response = string.Join("||", factures); // use || as a delimiter

            webView21.CoreWebView2.PostWebMessageAsString(response);
        }

        private List<string> SearchFacturesByDate(string date)
        {
            using (var context = new AppDbContext())
            {
                // Ensure valid date format
                if (string.IsNullOrWhiteSpace(date))
                    return new List<string> { "Date invalide" };

                // Get all matching FactureClients
                var clientFactures = context.factureClients
      .Include(f => f.entity)
      .Include(f => f.client)
      .Where(f => f.DateEcheance == date)
      .Select(f => $"*FC-{f.id} - Entité: {f.entity.Name} - Client: {(f.client != null ? f.client.Name : "Inconnu")} - Montant: {f.Total} - Statut: {f.Status}")
      .ToList();

                var fournisseurFactures = context.factureFournisseurs
                    .Include(f => f.entity)
                    .Include(f => f.fournisseur)
                    .Where(f => f.DateEcheance == date)
                    .Select(f => $"*FF-{f.id} - Entité: {f.entity.Name} - Fournisseur: {(f.fournisseur != null ? f.fournisseur.Name : "Inconnu")} - Montant: {f.Total} - Statut: {f.Status}")
                    .ToList();


                // Combine results
                var allFactures = clientFactures.Concat(fournisseurFactures).ToList();

                return allFactures.Count > 0 ? allFactures : new List<string> { "Aucune facture trouvée pour cette date." };
            }
        }




        private void webView21_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.OpenDevToolsWindow();

        }

        private void ParentControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (webView21.Bounds.Contains(e.Location))
            {
                // Alternative: Use Windows API to send the wheel message directly
                NativeMethods.SendMouseWheel(webView21.Handle, e.Delta);
            }
        }

        // Add this class for Windows API calls
        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            public static void SendMouseWheel(IntPtr hWnd, int delta)
            {
                const int WM_MOUSEWHEEL = 0x020A;
                int wParam = (delta << 16);
                SendMessage(hWnd, WM_MOUSEWHEEL, (IntPtr)wParam, IntPtr.Zero);
            }
        }

        private void webView21_MouseEnter(object sender, EventArgs e)
        {
            webView21.Focus();
        }

        private void webView21_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;

            if (m.Msg == WM_MOUSEWHEEL)
            {
                Point cursorPos = Cursor.Position;
                Point clientPos = this.PointToClient(cursorPos);
                Control target = GetChildAtPoint(clientPos);

                if (target is WebView2 webView)
                {
                    SendMessage(webView.Handle, m.Msg, m.WParam, m.LParam);
                    return; // Don't process further
                }
            }

            base.WndProc(ref m);
        }
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
