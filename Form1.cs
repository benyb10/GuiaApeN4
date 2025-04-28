using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuiaApeN4
{
    public partial class Gestor: Form
    {
        public Gestor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new GestionNotas().Show();
            this.Hide();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            // Mostrar un mensaje de confirmación antes de salir
            DialogResult resultado = MessageBox.Show("¿Está seguro de que desea salir?", "Confirmar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Si el usuario confirma, cerrar la aplicación
            if (resultado == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
