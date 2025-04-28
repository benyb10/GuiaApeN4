using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GuiaApeN4;

namespace GuiaApeN4
{
    public partial class GestionNotas : Form
    {
        public GestionNotas()
        {
            InitializeComponent();
            try
            {
                CargarEstudiantes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos iniciales: " + ex.Message);
            }
        }

     

        private void Limpiar() {
            txtNota1.Text = "";
            txtNota2.Text = "";
            txtSupletorio.Text = "";

        }

        private string CalcularEstado(decimal? nota1, decimal? nota2, decimal? supletorio)
        {
            decimal n1 = nota1 ?? 0;
            decimal n2 = nota2 ?? 0;
            decimal sup = supletorio ?? 0;

            if ((n1 + n2) >= 14)
            {
                return "APROBADO";
            }
            else if ((n1 + n2 + sup) >= 21)
            {
                return "APROBADO";
            }
            else
            {
                return "REPROBADO";
            }
        }

        private void ActualizarEstados()
        {
            using (var db = new RegistroNotasEntities())
            {
                var estudiantes = db.Notas.ToList();

                foreach (var estudiante in estudiantes)
                {
                    estudiante.Estado = CalcularEstado(estudiante.Nota1, estudiante.Nota2, estudiante.Supletorio);
                }

                db.SaveChanges(); // <--- Guarda todos los cambios en la base de datos
            }
        }
        private void CargarEstudiantes()
        {
            using (var db = new RegistroNotasEntities())
            {
                var lista = db.Notas
                    .Select(n => new
                    {
                        n.Cedula,
                        n.PrimerNombre,
                        n.PrimerApellido,
                        n.Nota1,
                        n.Nota2,
                        n.Supletorio,
                        n.Estado
                    })
                    .ToList();
                ActualizarEstados();

                dgvEstudiantes.DataSource = lista;
                dgvEstudiantes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                string criterio = txtBuscar.Text.Trim();

                using (var db = new RegistroNotasEntities())
                {
                    var lista = db.Notas
                        .Where(n => n.Cedula.Contains(criterio) || n.PrimerNombre.Contains(criterio))
                        .Select(n => new
                        {
                            n.Cedula,
                            n.PrimerNombre,
                            n.PrimerApellido,
                            n.Nota1,
                            n.Nota2,
                            n.Supletorio,
                            n.Estado
                        }).ToList();

                    dgvEstudiantes.DataSource = lista;
                    Limpiar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar estudiantes: " + ex.Message);
            }

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvEstudiantes.CurrentRow != null)
                {
                    string cedula = dgvEstudiantes.CurrentRow.Cells["Cedula"].Value.ToString();

                    using (var db = new RegistroNotasEntities())
                    {
                        var estudiante = db.Notas.FirstOrDefault(n => n.Cedula == cedula);
                        if (estudiante != null)
                        {
                            estudiante.Nota1 = Convert.ToDecimal(txtNota1.Text);
                            estudiante.Nota2 = Convert.ToDecimal(txtNota2.Text);
                            estudiante.Supletorio = Convert.ToDecimal(txtSupletorio.Text);

                            // 👉 Aquí calculas y actualizas el estado en el mismo momento
                            estudiante.Estado = CalcularEstado(estudiante.Nota1, estudiante.Nota2, estudiante.Supletorio);

                            db.SaveChanges();
                            MessageBox.Show("Notas guardadas correctamente.");

                            btnBuscar_Click(sender, e); // Opcional si quieres refrescar el datagrid
                            CargarEstudiantes();
                            Limpiar();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar notas: " + ex.Message);
            }
        }

        private void dgvEstudiantes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    txtNota1.Text = dgvEstudiantes.Rows[e.RowIndex].Cells["Nota1"].Value?.ToString() ?? "";
                    txtNota2.Text = dgvEstudiantes.Rows[e.RowIndex].Cells["Nota2"].Value?.ToString() ?? "";
                    txtSupletorio.Text = dgvEstudiantes.Rows[e.RowIndex].Cells["Supletorio"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar estudiante: " + ex.Message);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            btnGuardar_Click(sender, e);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvEstudiantes.CurrentRow != null)
                {
                    string cedula = dgvEstudiantes.CurrentRow.Cells["Cedula"].Value.ToString();

                    using (var db = new RegistroNotasEntities())
                    {
                        var estudiante = db.Notas.FirstOrDefault(n => n.Cedula == cedula);
                        if (estudiante != null)
                        {
                            estudiante.Nota1 = 0;
                            estudiante.Nota2 = 0;
                            estudiante.Supletorio = 0;
                            estudiante.Estado = "REPROBADO"; // También podrías usar tu método CalcularEstado(0,0,0)

                            db.SaveChanges();
                            MessageBox.Show("Notas eliminadas correctamente.");

                            btnBuscar_Click(sender, e); // Refresca si quieres buscar algo
                            CargarEstudiantes();        // 👉 Esta línea es importante para que se vea el nuevo estado
                            Limpiar();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar notas: " + ex.Message);
            }

        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            new Gestor().Show();
            this.Hide();
        }

        private void btnSupletorios_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new RegistroNotasEntities())
                {
                    int cantidadReprobados = db.Notas.Count(n => n.Estado == "REPROBADO");

                    MessageBox.Show("Número de estudiantes reprobados: " + cantidadReprobados, "Reprobados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al contar reprobados: " + ex.Message);
            }
        }

        private void btnPromedioCurso_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new RegistroNotasEntities())
                {
                    var estudiantes = db.Notas.ToList();

                    if (estudiantes.Count == 0)
                    {
                        MessageBox.Show("No hay estudiantes registrados.");
                        return;
                    }

                    double sumaTotal = 0;
                    int cantidadNotas = 0;

                    foreach (var estudiante in estudiantes)
                    {
                        // Validamos que las notas no sean mayores a 10
                        if (estudiante.Nota1 <= 10 && estudiante.Nota2 <= 10 && estudiante.Supletorio <= 10)
                        {
                            double notaFinal = Convert.ToDouble(estudiante.Nota1 + estudiante.Nota2 + estudiante.Supletorio);
                            sumaTotal += notaFinal;
                            cantidadNotas++;
                        }
                    }

                    if (cantidadNotas == 0)
                    {
                        MessageBox.Show("No hay notas válidas para calcular el promedio.");
                        return;
                    }

                    double promedio = sumaTotal / cantidadNotas;

                    MessageBox.Show("El promedio total del curso es: " + Math.Round(promedio, 2), "Promedio del Curso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al calcular el promedio: " + ex.Message);
            }

        }
    }
}
