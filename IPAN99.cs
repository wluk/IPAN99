using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        private static double dMin = -1;    //minimalna długość boku
        private static double dMax = 66;    //maksymalna długość boku

        static void Main(string[] args)
        {
            int[,] krzywa = new int[6, 2];         // tablica do przechowywania punktów
            Stopwatch timer = new Stopwatch(); // obiekt do sprawdzania czasu

            krzywa[0, 0] = 0;  //x1
            krzywa[0, 1] = 0;  //y1
            krzywa[1, 0] = 0;  //x2
            krzywa[1, 1] = 4;  //y2
            krzywa[2, 0] = 4;  //x3
            krzywa[2, 1] = 10;  //y3
            krzywa[3, 0] = 1;     //x4
            krzywa[3, 1] = 3;     //y4
            krzywa[4, 0] = 3;     //x5
            krzywa[4, 1] = 10;     //y5
            krzywa[5, 0] = 5;     //x6
            krzywa[5, 1] = 15;     //y6
            //krzywa[6, 0] = 3;
            //krzywa[6, 1] = 10;

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
                    int watek = int.Parse(Console.ReadLine());

                    timer.Start();
                    Oblicz(krzywa, watek);
                    timer.Stop();

                    break;
            }
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
                    punktPoprzedni = new Punkt()
                    {
                        x = krzywa[a - 1, 0],
                        y = krzywa[a - 1, 1]
                    },
                    punktSrodkowy = new Punkt()
                    {
                        x = krzywa[a, 0],
                        y = krzywa[a, 1]
                    },
                    punktNastepny = new Punkt()
                    {
                        x = krzywa[a + 1, 0],
                        y = krzywa[a + 1, 1]
                    }
                });
            };

            //OBLICZENIE ILOŚCI DANYCH NA WĄTEK
            int ilosc = trojkiPunktow.Count / liczbaWatkow;
            if (trojkiPunktow.Count % 2 != 0)
            {
                ilosc += 1;
            }

            //PRZYGOTOWANIE ZESTAWOW DANYCH DLA WĄTKÓW
            //lista na tablicę
            var tymczasowaTabelaTrojekPunktow = trojkiPunktow.ToArray();
            List<TrojkaPunktow[]> zbiorZestawowWatka = new List<TrojkaPunktow[]>();
            for (int i = 0; i < krzywa.GetLength(0); i++)
            {
                if (i % ilosc == 0)
                {
                    TrojkaPunktow[] zestawWatka = new TrojkaPunktow[ilosc];
                    for (int j = i; j < i + ilosc; j++)
                    {
                        if (j < tymczasowaTabelaTrojekPunktow.Length)
                        {
                            zestawWatka[j - i] = tymczasowaTabelaTrojekPunktow[j];
                        }
                    }
                    zbiorZestawowWatka.Add(zestawWatka);
                }
            }

            //WĄTKI
            List<double> wszystkieAlfy = new List<double>();
            List<Thread> wszystkieWatki = new List<Thread>();
            object alfa = null;
            foreach (var daneDlaWatka in zbiorZestawowWatka)
            {
                var thread = new Thread(
                  () =>
                  {
                      alfa = ZadanieWatka(daneDlaWatka);
                  });
                thread.Start();

                wszystkieWatki.Add(thread);
            }

            //oczekiwanie na koiec wszystkich watków
            foreach (var watek in wszystkieWatki)
            {
                watek.Join();
                wszystkieAlfy.AddRange((List<double>)alfa);
            }

            Console.WriteLine(wszystkieAlfy.Max());
        }

        /// <summary>
        /// Zadanie wykonywane przez wątek
        /// </summary>
        /// <param name="dane">Zestaw danych</param>
        /// <returns>Zwraca alfy obliczone dla zestawu danych</returns>
        private static List<double> ZadanieWatka(TrojkaPunktow[] dane)
        {
            List<double> alfyWatka = new List<double>();

            for (int i = 0; i < dane.GetLength(0); i++)
            {
                var alfa = Kat(dane[i].punktPoprzedni, dane[i].punktSrodkowy, dane[i].punktNastepny);
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
            if (CzyOdpowiedniOdcinek(dlugoscOdcinka1))
            {
                return 0;
            }

            double dlugoscOdcinka2 = DlugoscOdcinka(punktSrodkowy, punktNastepny);
            if (CzyOdpowiedniOdcinek(dlugoscOdcinka2))
            {
                return 0;
            }

            double dlugoscOdcinka3 = DlugoscOdcinka(punktPoprzedni, punktNastepny);
            if (CzyOdpowiedniOdcinek(dlugoscOdcinka3))
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
            bool relacja = false;
            if (dlugoscOdcinka > dMax)
            {
                Console.WriteLine("Długość odcinka jest większa od wartości maksymalnej");
                relacja = true;
            }

            if (dlugoscOdcinka < dMin)
            {
                Console.WriteLine("Długość odcinka jest mniejsza od wartości minimalnej");
                relacja = true;
            }

            return relacja;
        }

        /// <summary>
        /// Obliczenie długości odcinka za pomocą współrzednych
        /// </summary>
        /// <param name="A">punkt A</param>
        /// <param name="B">punkt B</param>
        /// <returns>Długość odcinka AB</returns>
        private static double DlugoscOdcinka(Punkt A, Punkt B)
        {
            double X = Math.Pow((B.x - A.x), 2);
            double Y = Math.Pow((B.y - A.y), 2);
            return Math.Sqrt(X + Y);
        }

        private class Punkt
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        private class TrojkaPunktow
        {
            public Punkt punktPoprzedni { get; set; }
            public Punkt punktSrodkowy { get; set; }
            public Punkt punktNastepny { get; set; }
        }
    }
}