using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Prueba_Android
{
    [Activity(Label = "MiActividad", MainLauncher = true)]
    public class MiActividad : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);
            Button checkInButton = FindViewById<Button>(Resource.Id.checkInButton);
            Button checkOutButton = FindViewById<Button>(Resource.Id.checkOutButton);

            checkInButton.Click += async (sender, e) =>
            {
                string codigo = inputText.Text;
                await RealizarCheckIn(codigo);
            };

            checkOutButton.Click += async (sender, e) =>
            {
                string codigo = inputText.Text;
                await RealizarCheckOut(codigo);
            };
        }

        private async Task RealizarCheckIn(string codigo)
        {
            // Aquí debes realizar la lógica para realizar la petición API para check in
            // Puedes utilizar HttpClient para realizar la petición HTTP
            // Por ejemplo:
            string apiUrl = "http://127.0.0.1:8000/api/v1/uniq/Practicante/" + codigo; 
            List<Practicante> jsonResponse = await GetApiResponse(apiUrl);
            string[] text = new string[11];
            // Aquí puedes trabajar con los datos, por ejemplo, mostrarlos en un cuadro de texto
            //textBox1.Text = jsonResponse[0];
            DateTime horaActual = DateTime.Now;
            string hora = horaActual.ToString("HH:mm:ss");
            foreach (var practicante in jsonResponse)
            {
                text[0] = practicante.id;
                text[1] = practicante.nombre + " " + practicante.apellidoP + " " + practicante.apellidoM;
                text[2] = practicante.carrera;
                text[3] = practicante.accessCode;
                text[4] = Convert.ToString(DateTime.Now);

            }
            List<PracticanteSimple> practicantesSimples = SeleccionarDatos(jsonResponse);
            await EnviarDatos(text);
        }

        private async Task EnviarDatos(string[] practicantes)
        {
            var formData = new MultipartFormDataContent();
            DateTime horaActual = DateTime.Now;
            DateTime fechaActual = DateTime.Now;
            string hora = horaActual.ToString("HH:mm:ss");
            string fecha = fechaActual.ToString("yyyy-MM-dd");
            // Agregar campos al formulario
            formData.Add(new StringContent(practicantes[1], Encoding.UTF8), "nombre_completo");
            formData.Add(new StringContent(practicantes[2], Encoding.UTF8), "carrera");
            formData.Add(new StringContent(practicantes[3], Encoding.UTF8), "codigo");
            formData.Add(new StringContent(fecha, Encoding.UTF8), "fecha");
            formData.Add(new StringContent(hora, Encoding.UTF8), "hora_llegada");
            formData.Add(new StringContent(hora, Encoding.UTF8), "hora_salida");

            formData.Add(new StringContent("0", Encoding.UTF8), "horas_trabajadas");
            formData.Add(new StringContent("0", Encoding.UTF8), "minutos_trabajados");
            formData.Add(new StringContent("0", Encoding.UTF8), "total_horas");
            formData.Add(new StringContent("0", Encoding.UTF8), "horas_extras");

            // Opcionalmente, agregar archivos al formulario
            //formData.Add(new ByteArrayContent(File.ReadAllBytes("archivo.txt")), "archivo", "archivo.txt");

            // Crear el cliente HTTP
            using (var client = new HttpClient())
            {
                try
                {
                    // Enviar la solicitud POST a la URL de destino
                    HttpResponseMessage response = await client.PostAsync("http://localhost:8000/api/v1/newUser/Practicante", formData);

                    // Leer la respuesta
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Imprimir la respuesta
                    Console.WriteLine(responseBody);

                    // Verificar si la solicitud fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("La solicitud fue exitosa.");
                    }
                    else
                    {
                        Console.WriteLine($"La solicitud falló con el código de estado: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private List<PracticanteSimple> SeleccionarDatos(List<Practicante> practicantes)
        {
            // Crear una lista de PracticanteSimple con solo los campos necesarios
            List<PracticanteSimple> practicantesSimples = new List<PracticanteSimple>();
            DateTime fechaActual = DateTime.Now;
            string fecha = fechaActual.ToString("yyyy-MM-dd");
            foreach (var practicante in practicantes)
            {
                practicantesSimples.Add(new PracticanteSimple
                {
                    nombre_completo = practicante.nombre + " " + practicante.apellidoP + " " + practicante.apellidoM,
                    carrera = practicante.carrera,
                    codigo = Convert.ToString(DateTime.Now),
                    fecha = fecha,
                    hora_llegada = Convert.ToString(DateTime.Now),
                    hora_salida = Convert.ToString(DateTime.Now),
                    horas_trabajadas = "0",
                    minutos_trabajados = "0",
                    total_horas = "0",
                    horas_extras = "0"

                    // Agregar otros campos necesarios
                });
            }
            return practicantesSimples;
        }

        private async Task<List<Practicante>> GetApiResponse(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Practicante>>(jsonResponse);
                }
                else
                {
                    throw new HttpRequestException($"La solicitud al API falló con el código de estado: {response.StatusCode}");
                }
            }
        }

        private async Task RealizarCheckOut(string codigo)
        {
            // Aquí debes realizar la lógica para realizar la petición API para check out
            // Puedes utilizar HttpClient para realizar la petición HTTP
            // Por ejemplo:
            using (HttpClient client = new HttpClient())
            {
                // Supongamos que la URL de la API es "http://tuservicio.com/api/checkout"
                string apiUrl = "http://127.0.0.1:8000/api/v1/modUser/Practicante/";

                // Supongamos que el cuerpo de la petición es un JSON con el código
                string jsonBody = $"{{ \"codigo\": \"{codigo}\" }}";
                StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Si la petición fue exitosa, puedes mostrar un mensaje de éxito
                    Toast.MakeText(this, "Check Out realizado correctamente", ToastLength.Short).Show();
                }
                else
                {
                    // Si la petición falló, puedes mostrar un mensaje de error
                    Toast.MakeText(this, "Error al realizar Check Out", ToastLength.Short).Show();
                }
            }
        }
    }

    public class Practicante
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string apellidoP { get; set; }
        public string apellidoM { get; set; }
        public string carrera { get; set; }
        public string accessCode { get; set; }
        public string practicas { get; set; }
        public string periodo { get; set; }
        public string horasN { get; set; }
        public string status { get; set; }
    }

    public class PracticanteSimple
    {
        public string nombre_completo { get; set; }
        public string carrera { get; set; }
        public string codigo { get; set; }
        public string fecha { get; set; }
        public string hora_llegada { get; set; }
        public string hora_salida { get; set; }
        public string horas_trabajadas { get; set; }
        public string minutos_trabajados { get; set; }
        public string total_horas { get; set; }
        public string horas_extras { get; set; }
    }
}