using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;

class Program{
    static void Main(){
        int opcion = 0;

        do{
            Console.WriteLine("\n===== CONVERSOR CSV - XML =====");
            Console.WriteLine("1. Convertir CSV a XML");
            Console.WriteLine("2. Convertir XML a CSV");
            Console.WriteLine("3. Salir");
            Console.Write("Elige una opción: ");

            int.TryParse(Console.ReadLine(), out opcion);

            switch (opcion){
                case 1:
                    ConvertirCSVXML();
                    break;

                case 2:
                    ConvertirXMLCSV();
                    break;

                case 3:
                    Console.WriteLine("Cerrando el programa...");
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

        } while (opcion != 3);
    }

  
    static void ConvertirCSVXML(){
        Console.Write("Introduce la ruta del archivo CSV: ");
        string rutaOrigen = Console.ReadLine();

        Console.Write("Introduce la ruta del archivo XML de destino: ");
        string rutaDestino = Console.ReadLine();

        if (!File.Exists(rutaOrigen)){
            Console.WriteLine("El archivo CSV no existe.");
            return;
        }

        try{
            string[][] peliculas = LeerCSV(rutaOrigen);

            Console.WriteLine("Películas leídas: " + peliculas.Length);

            MenuExportacion(peliculas, rutaOrigen, rutaDestino, true);        }
        catch (Exception e){
            Console.WriteLine("Error: " + e.Message);
        }
    }
    static void EscribirLog(string mensaje) {
    try {
        EventLog.WriteEntry("ConversorNetflix", mensaje,
            EventLogEntryType.Information);
    }
    catch (Exception e) {
        Console.WriteLine("Error al escribir el log: " + e.Message);
    }
}


    static void ConvertirXMLCSV(){
        Console.Write("Introduce la ruta del archivo XML: ");
        string rutaOrigen = Console.ReadLine();

        Console.Write("Introduce la ruta del archivo CSV de destino: ");
        string rutaDestino = Console.ReadLine();

        if (!File.Exists(rutaOrigen)){
            Console.WriteLine("El archivo XML no existe.");
            return;
        }

        try{
            string[][] peliculas = LeerXML(rutaOrigen);

            Console.WriteLine("Películas leídas: " + peliculas.Length);

            MenuExportacion(peliculas, rutaOrigen, rutaDestino, false);        }
        catch (Exception e){
            Console.WriteLine("Error: " + e.Message);
        }
    }


    static string[][] LeerCSV(string ruta){
        string[] lineas = File.ReadAllLines(ruta);

        int contador = 0;


        for (int i = 1; i < lineas.Length; i++){
            string[] campos = SepararCSV(lineas[i]);

            if (campos.Length == 6){
                contador++;
            }
        }


        string[][] peliculas = new string[contador][];

        contador = 0;


        for (int i = 1; i < lineas.Length; i++){
            string[] campos = SepararCSV(lineas[i]);

            if (campos.Length == 6){
                peliculas[contador] = campos;
                contador++;
            }
        }

        return peliculas;
    }


    static string[] SepararCSV(string linea){
        string[] campos = new string[6];

        StringBuilder campo = new StringBuilder();

        bool comillas = false;

        int contador = 0;

        for (int i = 0; i < linea.Length; i++){
            char caracter = linea[i];

            if (caracter == '"'){
                if (comillas && i + 1 < linea.Length &&
                    linea[i + 1] == '"'){
                    campo.Append('"');
                    i++;
                }
                else{
                    comillas = !comillas;
                }
            }
            else if (caracter == ',' && !comillas){
                if (contador >= campos.Length){
                    return new string[0];
                }

                campos[contador] = campo.ToString();
                contador++;

                campo.Clear();
            }
            else{
                campo.Append(caracter);
            }
        }

        if (comillas || contador >= campos.Length){
            return new string[0];
        }

        campos[contador] = campo.ToString();
        contador++;

        if (contador != 6){
            return new string[0];
        }

        return campos;
    }



    static string[][] LeerXML(string ruta){
        XmlDocument documento = new XmlDocument();

        documento.Load(ruta);

        XmlNode raiz = documento.DocumentElement;

        if (raiz == null || raiz.Name != "Peliculas"){
            throw new Exception("El XML no tiene la estructura correcta.");
        }

        int contador = 0;


        foreach (XmlNode pelicula in raiz.ChildNodes){
            if (pelicula.Name == "Pelicula"){
                contador++;
            }
        }

        string[][] peliculas = new string[contador][];

        contador = 0;


        foreach (XmlNode pelicula in raiz.ChildNodes){
            if (pelicula.Name == "Pelicula"){
                string[] campos = new string[6];

                campos[0] = ObtenerValorXML(pelicula, "Title");
                campos[1] = ObtenerValorXML(pelicula, "Genre");
                campos[2] = ObtenerValorXML(pelicula, "Premiere");
                campos[3] = ObtenerValorXML(pelicula, "Runtime");
                campos[4] = ObtenerValorXML(pelicula, "IMDBScore");
                campos[5] = ObtenerValorXML(pelicula, "Language");

                peliculas[contador] = campos;

                contador++;
            }
        }

        return peliculas;
    }


    static string ObtenerValorXML(XmlNode pelicula, string etiqueta){
        foreach (XmlNode elemento in pelicula.ChildNodes){
            if (elemento.Name == etiqueta){
                return elemento.InnerText;
            }
        }

        return "";
    }

static void MenuExportacion(string[][] peliculas, string rutaOrigen,
    string rutaDestino, bool xml){        
        int opcion = 0;

        do{
            Console.WriteLine("\n===== FILTRAR PELÍCULAS =====");
            Console.WriteLine("1. Exportar todas las películas");
            Console.WriteLine("2. Exportar por género");
            Console.WriteLine("3. Exportar por año");
            Console.WriteLine("4. Exportar por idioma");
            Console.WriteLine("5. Volver");
            Console.Write("Elige una opción: ");

            int.TryParse(Console.ReadLine(), out opcion);

            string[][] resultado = peliculas;

            switch (opcion){
                case 1:
                    resultado = peliculas;
                    break;

                case 2:
                    resultado = ElegirValor(peliculas, 1);
                    break;

                case 3:
                    resultado = FiltrarAnyo(peliculas);
                    break;

                case 4:
                    resultado = ElegirValor(peliculas, 5);
                    break;

                case 5:
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            if (opcion >= 1 && opcion <= 4){
                GuardarDatos(resultado, rutaOrigen, rutaDestino, xml);            
                }

        } while (opcion != 5);
    }

    static string[] ObtenerValoresUnicos(string[][] peliculas, int posicion){
        int contador = 0;


        for (int i = 0; i < peliculas.Length; i++){
            bool repetido = false;

            for (int j = 0; j < i; j++){
                if (String.Compare(peliculas[i][posicion],peliculas[j][posicion],true) == 0){
                    repetido = true;
                    break;
                }
            }

            if (!repetido){
                contador++;
            }
        }

        string[] valores = new string[contador];

        contador = 0;


        for (int i = 0; i < peliculas.Length; i++){
            bool repetido = false;

            for (int j = 0; j < i; j++){
                if (String.Compare(peliculas[i][posicion],peliculas[j][posicion],true) == 0){
                    repetido = true;
                    break;
                }
            }

            if (!repetido){
                valores[contador] = peliculas[i][posicion];
                contador++;
            }
        }

        return valores;
    }

    static string[][] ElegirValor(string[][] peliculas, int posicion){
        string[] valores = ObtenerValoresUnicos(peliculas, posicion);

        if (valores.Length == 0){
            Console.WriteLine("No hay valores disponibles.");
            return new string[0][];
        }

        Console.WriteLine("\nValores disponibles:");

        for (int i = 0; i < valores.Length; i++){
            Console.WriteLine((i + 1) + ". " + valores[i]);
        }

        Console.Write("Selecciona un valor: ");

        int opcion;

        if (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > valores.Length){
            Console.WriteLine("Selección no válida.");
            return new string[0][];
        }

        return FiltrarCampo(peliculas, posicion, valores[opcion - 1]);
    }

    static string[][] FiltrarCampo(string[][] peliculas,int posicion,string valor){
        int contador = 0;


        for (int i = 0; i < peliculas.Length; i++){
            if (String.Compare(peliculas[i][posicion], valor, true) == 0){
                contador++;
            }
        }

        string[][] resultado = new string[contador][];

        contador = 0;


        for (int i = 0; i < peliculas.Length; i++){
            if (String.Compare(peliculas[i][posicion], valor, true) == 0){
                resultado[contador] = peliculas[i];
                contador++;
            }
        }

        return resultado;
    }


    static string[][] FiltrarAnyo(string[][] peliculas){
        Console.Write("Introduce el año: ");

        int anio;

        if (!int.TryParse(Console.ReadLine(), out anio)){
            Console.WriteLine("El año introducido no es válido.");
            return new string[0][];
        }

        int contador = 0;

        for (int i = 0; i < peliculas.Length; i++){
            int anyoPelicula = ObtenerAnyo(peliculas[i][2]);

            if (anyoPelicula >= anio){
                contador++;
            }
        }

        string[][] resultado = new string[contador][];

        contador = 0;


        for (int i = 0; i < peliculas.Length; i++){
            int anyoPelicula = ObtenerAnyo(peliculas[i][2]);

            if (anyoPelicula >= anio){
                resultado[contador] = peliculas[i];
                contador++;
            }
        }

        return resultado;
    }


    static int ObtenerAnyo(string fecha){
        if (fecha.Length < 4){
            return 0;
        }

        string textoAnyo = fecha.Substring(fecha.Length - 4, 4);

        int anyo;

        if (int.TryParse(textoAnyo, out anyo)){
            return anyo;
        }

        return 0;
    }

    static void GuardarDatos(string[][] peliculas, string rutaOrigen,
    string rutaDestino, bool xml) {

    string formatoOrigen = "CSV";
    string formatoDestino = "XML";

    if (!xml) {
        formatoOrigen = "XML";
        formatoDestino = "CSV";
    }

    EscribirLog("INICIO DE EXPORTACIÓN\n" +
        "Formato de origen: " + formatoOrigen + "\n" +
        "Ruta de origen: " + rutaOrigen + "\n" +
        "Formato de destino: " + formatoDestino + "\n" +
        "Ruta de destino: " + rutaDestino);

    for (int i = 0; i < peliculas.Length; i++) {
        EscribirLog("Película " + (i + 1) + "\n" +
            "Título: " + peliculas[i][0] + "\n" +
            "Género: " + peliculas[i][1] + "\n" +
            "Estreno: " + peliculas[i][2] + "\n" +
            "Duración: " + peliculas[i][3] + "\n" +
            "Puntuación IMDB: " + peliculas[i][4] + "\n" +
            "Idioma: " + peliculas[i][5]);
    }

    if (xml) {
        GuardarXML(peliculas, rutaDestino);
    }
    else {
        GuardarCSV(peliculas, rutaDestino);
    }

    EscribirLog("EXPORTACIÓN FINALIZADA\n" +
        "Películas exportadas: " + peliculas.Length + "\n" +
        "Archivo creado en: " + rutaDestino);

    Console.WriteLine("Archivo guardado correctamente.");
    Console.WriteLine("Películas exportadas: " + peliculas.Length);
}

    static void GuardarXML(string[][] peliculas, string ruta){
        XmlWriter escritor = XmlWriter.Create(ruta);

        escritor.WriteStartDocument();

        escritor.WriteStartElement("Peliculas");

        for (int i = 0; i < peliculas.Length; i++){
            escritor.WriteStartElement("Pelicula");

            escritor.WriteStartElement("Title");
            escritor.WriteString(peliculas[i][0]);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Genre");
            escritor.WriteString(peliculas[i][1]);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Premiere");
            escritor.WriteString(peliculas[i][2]);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Runtime");
            escritor.WriteString(peliculas[i][3]);
            escritor.WriteEndElement();

            escritor.WriteStartElement("IMDBScore");
            escritor.WriteString(peliculas[i][4]);
            escritor.WriteEndElement();

            escritor.WriteStartElement("Language");
            escritor.WriteString(peliculas[i][5]);
            escritor.WriteEndElement();

            escritor.WriteEndElement();
        }

        escritor.WriteEndElement();

        escritor.WriteEndDocument();

        escritor.Close();
    }


    static void GuardarCSV(string[][] peliculas, string ruta){
        StringBuilder contenido = new StringBuilder();

        contenido.AppendLine("Title,Genre,Premiere,Runtime,IMDB Score,Language");

        for (int i = 0; i < peliculas.Length; i++){
            for (int j = 0; j < peliculas[i].Length; j++){
                contenido.Append(EscaparCSV(peliculas[i][j]));

                if (j < peliculas[i].Length - 1){
                    contenido.Append(",");
                }
            }

            contenido.AppendLine();
        }

        File.WriteAllText(ruta, contenido.ToString());
    }

    static string EscaparCSV(string texto){
        if (texto.Contains(",") || texto.Contains("\"") || texto.Contains("\n") || texto.Contains("\r")){
            texto = texto.Replace("\"", "\"\"");

            texto = "\"" + texto + "\"";
        }

        return texto;
    }
}