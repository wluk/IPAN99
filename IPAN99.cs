using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        private static double _dMin = -1;    //minimalna długość boku
        private static double _dMax = 66;    //maksymalna długość boku

        static void Main(string[] args)
        {
            int[,] krzywa = new int[7, 2];         // tablica do przechowywania punktów
            Stopwatch timer = new Stopwatch(); // obiekt do sprawdzania czasu
            krzywa[0, 0] = 0;   //x1
            krzywa[0, 1] = 0;   //y1
            krzywa[1, 0] = 1;   //x2
            krzywa[1, 1] = 1;   //y2
            krzywa[2, 0] = 2;   //x3
            krzywa[2, 1] = 2;   //y3
            krzywa[3, 0] = 3;   //x4
            krzywa[3, 1] = 3;   //y4
            krzywa[4, 0] = 4;   //x5
            krzywa[4, 1] = 4;   //y5
            krzywa[5, 0] = 5;   //x6
            krzywa[5, 1] = 5;   //y6
            krzywa[6, 0] = 6;
            krzywa[6, 1] = 6;

            Console.WriteLine("Algorytm IPAN99 - obliczania na watkach");
            Console.WriteLine();
            Console.WriteLine("Menu - wybierz opcje");
            Console.WriteLine("Program jednowątkowy - 1");
            Console.WriteLine("Program wielowątkowy - 2");
            Console.Write("\nWybieram opcje numer: ");
            int select = int.Parse(Console.ReadLine());
            Console.Clear();

            switch (select)
            {
                case 1:
                    timer.Start();
                    Oblicz(krzywa, 1);
                    timer.Stop();

                    break;
                case 2:
                    Console.WriteLine("Wybrałeś opcje wielowątkowa podaj na ilu wątkach chcesz dokodonać obliczeń");
                    Console.Write("Podaj ilość wątków: ");
                    int liczbaWatkow = int.Parse(Console.ReadLine());

                    timer.Start();
                    Oblicz(krzywa, liczbaWatkow);
                    timer.Stop();

                    break;
            }

            Console.WriteLine("Czas obliczania: {0}ms", timer.ElapsedMilliseconds);
            Console.ReadKey();

        }

        /// <summary>
        /// Metoda oblicza alfy dla punktów krzywej
        /// </summary>
        /// <param name="krzywa">Zbiór punktów</param>
        /// <param name="liczbaWatkow">Ilość wątków</param>
        /// <returns>Największa alfa</returns>
        private static void Oblicz(int[,] krzywa, int liczbaWatkow)
        {
            //PODZIAŁ KRZYWEJ NA TRÓJKI OBLICZENIOWE
            //zestawy 3 punktów do obliczenia alfy
            List<TrojkaPunktow> trojkiPunktow = new List<TrojkaPunktow>();

            for (int a = 1; a < krzywa.GetLength(0) - 1; a++) // ochrona przed wyjsciem z tablicy
            {
                trojkiPunktow.Add(new TrojkaPunktow()
                {
                    PunktPoprzedni = new Punkt()
                    {
                        X = krzywa[a - 1, 0],
                        Y = krzywa[a - 1, 1]
                    },
                    PunktSrodkowy = new Punkt()
                    {
                        X = krzywa[a, 0],
                        Y = krzywa[a, 1]
                    },
                    PunktNastepny = new Punkt()
                    {
                        X = krzywa[a + 1, 0],
                        Y = krzywa[a + 1, 1]
                    }
                });
            }
            //OBLICZENIE ILOŚCI DANYCH NA WĄTEK
            int ilosc = trojkiPunktow.Count / liczbaWatkow;
            if (trojkiPunktow.Count % 2 != 0)
            {
                ilosc += 1;
            }

            //PRZYGOTOWANIE ZESTAWOW DANYCH DLA WĄTKÓW
            var zbiorZestawowWatka = new List<List<TrojkaPunktow>>();
            var zestaw = new List<TrojkaPunktow>();
            foreach (var trojka in trojkiPunktow)
            {
                zestaw.Add(trojka);
                if (zestaw.Count == ilosc || trojka == trojkiPunktow.Last())
                {
                    zbiorZestawowWatka.Add(zestaw);
                    zestaw = new List<TrojkaPunktow>();
                }
            }

            //WĄTKI
            var wszystkieAlfy = new List<double>();
            var wszystkieWatki = new List<Thread>();

            foreach (var daneDlaWatka in zbiorZestawowWatka)
            {
                var thread = new Thread(
                  () =>
                  {
                      wszystkieAlfy.AddRange(ZadanieWatka(daneDlaWatka));
                      
                  });
                thread.IsBackground = true;
                thread.Start();

                wszystkieWatki.Add(thread);
            }

            //oczekiwanie na koiec wszystkich watków
            foreach (var watek in wszystkieWatki)
            {
                watek.Join();
                //wszystkieAlfy.AddRange((List<double>)alfa);
            }

            Console.WriteLine(wszystkieAlfy.Max());
        }

        /// <summary>
        /// Zadanie wykonywane przez wątek
        /// </summary>
        /// <param name="zestawDanych">Zestaw danych</param>
        /// <returns>Zwraca alfy obliczone dla zestawu danych</returns>
        private static List<double> ZadanieWatka(List<TrojkaPunktow> zestawDanych)
        {
            var alfyWatka = new List<double>();

            foreach (var trojka in zestawDanych)
            {
                var alfa = Kat(trojka.PunktPoprzedni, trojka.PunktSrodkowy, trojka.PunktNastepny);
                alfyWatka.Add(alfa);
            }

            return alfyWatka;
        }

        /// <summary>
        /// Metoda obliczająca krzywiznę na podstawie punktów
        /// </summary>
        /// <param name="punktPoprzedni"></param>
        /// <param name="punktSrodkowy"></param>
        /// <param name="punktNastepny"></param>
        /// <returns>Krzywizna pomiedzy punktami</returns>
        private static double Kat(Punkt punktPoprzedni, Punkt punktSrodkowy, Punkt punktNastepny)
        {
            double dlugoscOdcinka1 = DlugoscOdcinka(punktPoprzedni, punktSrodkowy);
            if (!CzyOdpowiedniOdcinek(dlugoscOdcinka1))
            {
                return 0;
            }

            double dlugoscOdcinka2 = DlugoscOdcinka(punktSrodkowy, punktNastepny);
            if (!CzyOdpowiedniOdcinek(dlugoscOdcinka2))
            {
                return 0;
            }

            double dlugoscOdcinka3 = DlugoscOdcinka(punktPoprzedni, punktNastepny);
            if (!CzyOdpowiedniOdcinek(dlugoscOdcinka3))
            {
                return 0;
            }

            double alfa = Math.Acos((dlugoscOdcinka1 * dlugoscOdcinka1 + dlugoscOdcinka2 * dlugoscOdcinka2 - dlugoscOdcinka3 * dlugoscOdcinka3) / (2 * dlugoscOdcinka1 * dlugoscOdcinka2));
            alfa = alfa * 180 / Math.PI;

            Console.Write("Alfa wynosi: ");
            Console.WriteLine(alfa);

            return alfa;
        }

        /// <summary>
        /// Informacja o niezgodności danych
        /// </summary>
        /// <param name="dlugoscOdcinka"></param>
        private static bool CzyOdpowiedniOdcinek(double dlugoscOdcinka)
        {
            bool odpowiedni = true;
            if (dlugoscOdcinka > _dMax)
            {
                Console.WriteLine("Długość odcinka jest większa od wartości maksymalnej");
                odpowiedni = false;
            }

            if (dlugoscOdcinka < _dMin)
            {
                Console.WriteLine("Długość odcinka jest mniejsza od wartości minimalnej");
                odpowiedni = false;
            }

            return odpowiedni;
        }

        /// <summary>
        /// Obliczenie długości odcinka za pomocą współrzednych
        /// </summary>
        /// <param name="a">punkt A</param>
        /// <param name="b">punkt B</param>
        /// <returns>Długość odcinka AB</returns>
        private static double DlugoscOdcinka(Punkt a, Punkt b)
        {
            double x = Math.Pow((b.X - a.X), 2);
            double y = Math.Pow((b.Y - a.Y), 2);
            return Math.Sqrt(x + y);
        }

        private class Punkt
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        private class TrojkaPunktow
        {
            public Punkt PunktPoprzedni { get; set; }
            public Punkt PunktSrodkowy { get; set; }
            public Punkt PunktNastepny { get; set; }
        }
    }
}