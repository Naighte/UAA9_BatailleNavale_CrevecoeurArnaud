using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetBatailleNavale2026
{
    internal class MethodesDuProjet
    {
        public struct Coordonnee
        {
            public int Ligne;   // 0-7  → A-H
            public int Colonne; // 0-8  → 1-9

            public Coordonnee(int ligne, int colonne)
            {
                Ligne = ligne;
                Colonne = colonne;
            }

            public override string ToString() =>
                $"{(char)('A' + Ligne)}{Colonne + 1}";
        }

        public struct Bateau
        {
            public string Nom;
            public int Taille;
            public List<Coordonnee> Cases;
            public List<Coordonnee> CasesTouchees;

            public Bateau(string nom, int taille)
            {
                Nom = nom;
                Taille = taille;
                Cases = new List<Coordonnee>();
                CasesTouchees = new List<Coordonnee>();
            }

            public bool EstCoule() => CasesTouchees.Count >= Cases.Count;
        }

        public struct Joueur
        {
            public string Nom;
            public bool EstBot;
            public char[,] Grille;           // grille personnelle (bateaux)
            public char[,] GrilleAdversaire; // grille de tir (ce que le joueur voit)
            public List<Bateau> Bateaux;

            public Joueur(string nom, bool estBot)
            {
                Nom = nom;
                EstBot = estBot;
                Grille = new char[8, 9];
                GrilleAdversaire = new char[8, 9];
                Bateaux = new List<Bateau>();

                for (int l = 0; l < 8; l++)
                    for (int c = 0; c < 9; c++)
                    {
                        Grille[l, c] = '~';
                        GrilleAdversaire[l, c] = '~';
                    }
            }

            public bool TousBateauxCoules()
            {
                foreach (var b in Bateaux)
                    if (!b.EstCoule()) return false;
                return true;
            }
        }

        // ── LANCEMENT DE PARTIE ──────────────────────────────────────────

        public static Random rng = new Random();

        // Définition des bateaux (nom, taille) – inspiré de l'image
        static (string nom, int taille)[] BATEAUX_DEF =
        {
            ("Porte-avions", 5),
            ("Croiseur",     4),
            ("Destroyer",    3),
            ("Sous-marin",   3),
            ("Torpilleur",   2),
        };

        public static void LancerPartie()
        {
            // --- Choix adversaire ---
            Console.Clear();
            AfficherBanniere();
            EcrireLigne("═══════════════════════════════════", COULEUR_TITRE);
            EcrireLigne("  1. Jouer contre un ami (tour/tour)", COULEUR_INFO);
            EcrireLigne("  2. Jouer contre le Bot           ", COULEUR_INFO);
            EcrireLigne("═══════════════════════════════════", COULEUR_TITRE);
            Ecrire(" Votre choix : ", COULEUR_TITRE);

            bool contreBot = false;
            while (true)
            {
                string saisie = Console.ReadLine()?.Trim();
                if (saisie == "1") { contreBot = false; break; }
                if (saisie == "2") { contreBot = true; break; }
                Ecrire(" Entrée invalide, 1 ou 2 : ", ConsoleColor.Red);
            }

            // --- Choix mode ---
            Console.Clear();
            AfficherBanniere();
            EcrireLigne("═══════════════════════════════════════════════════════════", COULEUR_TITRE);
            EcrireLigne("  1. Mode Express  (placement aléatoire, tirs continus)   ", COULEUR_INFO);
            EcrireLigne("  2. Mode Normal   (placement manuel, 1 tir par tour)     ", COULEUR_INFO);
            EcrireLigne("═══════════════════════════════════════════════════════════", COULEUR_TITRE);
            Ecrire(" Votre choix : ", COULEUR_TITRE);

            bool modeExpress = false;
            while (true)
            {
                string saisie = Console.ReadLine()?.Trim();
                if (saisie == "1") { modeExpress = true; break; }
                if (saisie == "2") { modeExpress = false; break; }
                Ecrire(" Entrée invalide, 1 ou 2 : ", ConsoleColor.Red);
            }

            // --- Création des joueurs ---
            string nomJ1 = DemanderNom("Joueur 1");
            string nomJ2 = contreBot ? "Bot" : DemanderNom("Joueur 2");

            Joueur j1 = new Joueur(nomJ1, false);
            Joueur j2 = new Joueur(nomJ2, contreBot);

            // --- Placement des bateaux ---
            PlacerBateaux(ref j1, modeExpress);
            PlacerBateaux(ref j2, modeExpress);

            // --- Boucle de jeu ---
            BoucleDeJeu(ref j1, ref j2, contreBot, modeExpress);
        }

        // ── PLACEMENT ────────────────────────────────────────────────────

        public static void PlacerBateaux(ref Joueur joueur, bool express)
        {
            // Initialiser la liste de bateaux
            joueur.Bateaux = new List<Bateau>();
            foreach (var def in BATEAUX_DEF)
                joueur.Bateaux.Add(new Bateau(def.nom, def.taille));

            if (express || joueur.EstBot)
            {
                PlacerAleatoire(ref joueur);
                return;
            }

            // Placement manuel
            Console.Clear();
            EcrireLigne($"\n ⚓  {joueur.Nom}, placez vos bateaux !\n", COULEUR_TITRE);

            for (int i = 0; i < joueur.Bateaux.Count; i++)
            {
                Bateau b = joueur.Bateaux[i];
                bool place = false;

                while (!place)
                {
                    AfficherGrille(joueur.Grille, $"Grille de {joueur.Nom}", true);
                    EcrireLigne($"\n Bateau : {b.Nom} (taille {b.Taille})", COULEUR_TITRE);

                    Coordonnee origine = DemanderCoordonnee($" Coordonnée d'origine");
                    bool horizontal = DemanderDirection();

                    List<Coordonnee> cases = GenererCases(origine, b.Taille, horizontal);
                    if (cases == null)
                    {
                        EcrireLigne(" ✗ Hors limites !", ConsoleColor.Red);
                        Pause();
                        continue;
                    }
                    if (ChevaucheExistant(joueur.Grille, cases))
                    {
                        EcrireLigne(" ✗ Chevauchement détecté !", ConsoleColor.Red);
                        Pause();
                        continue;
                    }

                    b.Cases = cases;
                    joueur.Bateaux[i] = b;
                    foreach (var c in cases) joueur.Grille[c.Ligne, c.Colonne] = 'B';
                    place = true;
                }
            }
        }

        public static void PlacerAleatoire(ref Joueur joueur)
        {
            for (int i = 0; i < joueur.Bateaux.Count; i++)
            {
                Bateau b = joueur.Bateaux[i];
                List<Coordonnee> cases = null;

                while (cases == null || ChevaucheExistant(joueur.Grille, cases))
                {
                    int ligne = rng.Next(0, 8);
                    int colonne = rng.Next(0, 9);
                    bool horiz = rng.Next(2) == 0;
                    cases = GenererCases(new Coordonnee(ligne, colonne), b.Taille, horiz);
                }

                b.Cases = cases;
                joueur.Bateaux[i] = b;
                foreach (var c in cases) joueur.Grille[c.Ligne, c.Colonne] = 'B';
            }
        }

        // ── BOUCLE DE JEU ────────────────────────────────────────────────

        public static void BoucleDeJeu(ref Joueur j1, ref Joueur j2, bool contreBot, bool modeExpress)
        {
            bool tourJ1 = true;

            while (true)
            {
                if (tourJ1)
                {
                    JouerTour(ref j1, ref j2, modeExpress, false);
                    if (j2.TousBateauxCoules()) { AnnoncerVainqueur(j1); return; }
                }
                else
                {
                    JouerTour(ref j2, ref j1, modeExpress, contreBot);
                    if (j1.TousBateauxCoules()) { AnnoncerVainqueur(j2); return; }
                }

                tourJ1 = !tourJ1;

                // En mode 2 joueurs sur même PC, masquer la grille entre les tours
                if (!contreBot)
                {
                    Ecrire($"\n Appuyez sur [Entrée] pour passer le tour à l'autre joueur...", COULEUR_INFO);
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        public static void JouerTour(ref Joueur tireur, ref Joueur cible, bool modeExpress, bool estBot)
        {
            Console.Clear();
            AfficherBanniere();
            EcrireLigne($"\n ► Tour de {tireur.Nom}\n", COULEUR_TITRE);

            AfficherDeuxGrilles(tireur, cible);

            if (modeExpress)
                TourExpress(ref tireur, ref cible, estBot);
            else
                TourNormal(ref tireur, ref cible, estBot);
        }

        // -- Mode Normal : 1 tir --
        public static void TourNormal(ref Joueur tireur, ref Joueur cible, bool estBot)
        {
            Coordonnee coord;
            if (estBot)
                coord = TirBot(tireur.GrilleAdversaire);
            else
                coord = DemanderCoordonnee(" Entrez vos coordonnées de tir");

            ResultatTir(ref tireur, ref cible, coord);
            Pause();
        }

        // -- Mode Express : tirs continus sur un bateau touché --
        public static void TourExpress(ref Joueur tireur, ref Joueur cible, bool estBot)
        {
            // Trouver s'il y a une case touchée non coulée à continuer
            Coordonnee? cibleContinue = TrouverContinuation(tireur.GrilleAdversaire);

            while (true)
            {
                Coordonnee coord;

                if (estBot)
                    coord = cibleContinue.HasValue
                        ? TirBotContinuation(tireur.GrilleAdversaire, cibleContinue.Value)
                        : TirBot(tireur.GrilleAdversaire);
                else
                {
                    if (cibleContinue.HasValue)
                        EcrireLigne($" 🎯 Bateau en cours ! Continuez de tirer !", ConsoleColor.Yellow);
                    coord = DemanderCoordonnee(" Entrez vos coordonnées de tir");
                }

                bool touche = ResultatTir(ref tireur, ref cible, coord);

                if (cible.TousBateauxCoules()) return; // fin de partie

                if (!touche)
                {
                    EcrireLigne(" À l'eau ! Fin du tour.", COULEUR_RATE);
                    Pause();
                    return; // tir raté → fin du tour
                }

                // Vérifier si le bateau touché est coulé
                Bateau? batTouche = TrouverBateauTouche(cible, coord);
                if (batTouche.HasValue && batTouche.Value.EstCoule())
                {
                    EcrireLigne($" ✓ {batTouche.Value.Nom} coulé ! Fin du tour.", COULEUR_SUCCES);
                    Pause();
                    return;
                }

                cibleContinue = TrouverContinuation(tireur.GrilleAdversaire);
                AfficherDeuxGrilles(tireur, cible);
            }
        }

        // ── TIR ──────────────────────────────────────────────────────────

        public static bool ResultatTir(ref Joueur tireur, ref Joueur cible, Coordonnee coord)
        {
            char etatActuel = tireur.GrilleAdversaire[coord.Ligne, coord.Colonne];
            if (etatActuel == 'X' || etatActuel == 'O')
            {
                EcrireLigne(" ✗ Case déjà ciblée !", ConsoleColor.Red);
                return false;
            }

            bool touche = cible.Grille[coord.Ligne, coord.Colonne] == 'B';

            if (touche)
            {
                tireur.GrilleAdversaire[coord.Ligne, coord.Colonne] = 'X';
                cible.Grille[coord.Ligne, coord.Colonne] = 'X';

                // Mettre à jour les cases touchées dans le bateau
                for (int i = 0; i < cible.Bateaux.Count; i++)
                {
                    Bateau b = cible.Bateaux[i];
                    foreach (var c in b.Cases)
                    {
                        if (c.Ligne == coord.Ligne && c.Colonne == coord.Colonne)
                        {
                            b.CasesTouchees.Add(coord);
                            cible.Bateaux[i] = b;

                            if (b.EstCoule())
                            {
                                EcrireLigne($"\n 💥 {coord} → TOUCHÉ ! {b.Nom} COULÉ !", COULEUR_TOUCHE);
                                // Marquer tout le bateau coulé sur la grille adverse
                                foreach (var cc in b.Cases)
                                    tireur.GrilleAdversaire[cc.Ligne, cc.Colonne] = 'X';
                            }
                            else
                                EcrireLigne($"\n 🔥 {coord} → TOUCHÉ !", COULEUR_TOUCHE);
                            return true;
                        }
                    }
                }
            }
            else
            {
                tireur.GrilleAdversaire[coord.Ligne, coord.Colonne] = 'O';
                cible.Grille[coord.Ligne, coord.Colonne] = 'O';
                EcrireLigne($"\n 💧 {coord} → À L'EAU !", COULEUR_RATE);
            }

            return touche;
        }

        // ── BOT ───────────────────────────────────────────────────────────

        public static Coordonnee TirBot(char[,] grille)
        {
            int l, c;
            do
            {
                l = rng.Next(0, 8);
                c = rng.Next(0, 9);
            } while (grille[l, c] == 'X' || grille[l, c] == 'O');

            Coordonnee coord = new Coordonnee(l, c);
            EcrireLigne($" 🤖 Le Bot tire en {coord}", COULEUR_INFO);
            System.Threading.Thread.Sleep(800);
            return coord;
        }

        public static Coordonnee TirBotContinuation(char[,] grille, Coordonnee ref_)
        {
            int[] dl = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            // chercher autour des X
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 9; c++)
                {
                    if (grille[l, c] != 'X') continue;
                    for (int d = 0; d < 4; d++)
                    {
                        int nl = l + dl[d];
                        int nc = c + dc[d];
                        if (nl >= 0 && nl < 8 && nc >= 0 && nc < 9
                            && grille[nl, nc] != 'X' && grille[nl, nc] != 'O')
                        {
                            Coordonnee coord = new Coordonnee(nl, nc);
                            EcrireLigne($" 🤖 Le Bot tire en {coord}", COULEUR_INFO);
                            System.Threading.Thread.Sleep(800);
                            return coord;
                        }
                    }
                }
            return TirBot(grille);
        }

        // ── HELPERS ───────────────────────────────────────────────────────

        public static List<Coordonnee> GenererCases(Coordonnee origine, int taille, bool horizontal)
        {
            var cases = new List<Coordonnee>();
            for (int i = 0; i < taille; i++)
            {
                int l = origine.Ligne + (horizontal ? 0 : i);
                int c = origine.Colonne + (horizontal ? i : 0);
                if (l >= 8 || c >= 9) return null;
                cases.Add(new Coordonnee(l, c));
            }
            return cases;
        }

        public static bool ChevaucheExistant(char[,] grille, List<Coordonnee> cases)
        {
            foreach (var c in cases)
                if (grille[c.Ligne, c.Colonne] == 'B') return true;
            return false;
        }

        public static Coordonnee? TrouverContinuation(char[,] grille)
        {
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 9; c++)
                    if (grille[l, c] == 'X') return new Coordonnee(l, c);
            return null;
        }

        public static Bateau? TrouverBateauTouche(Joueur cible, Coordonnee coord)
        {
            foreach (var b in cible.Bateaux)
                foreach (var c in b.Cases)
                    if (c.Ligne == coord.Ligne && c.Colonne == coord.Colonne)
                        return b;
            return null;
        }

        // ── SAISIES ───────────────────────────────────────────────────────

        public static string DemanderNom(string label)
        {
            Ecrire($" Nom du {label} : ", COULEUR_TITRE);
            string nom = Console.ReadLine()?.Trim();
            return string.IsNullOrEmpty(nom) ? label : nom;
        }

        public static Coordonnee DemanderCoordonnee(string prompt)
        {
            while (true)
            {
                Ecrire($"{prompt} (ex: A 3 ou A3) : ", COULEUR_INFO);
                string saisie = Console.ReadLine()?.Trim().ToUpper() ?? "";
                saisie = saisie.Replace(" ", "");

                if (saisie.Length >= 2)
                {
                    char lettre = saisie[0];
                    if (lettre >= 'A' && lettre <= 'H'
                        && int.TryParse(saisie.Substring(1), out int col)
                        && col >= 1 && col <= 9)
                    {
                        return new Coordonnee(lettre - 'A', col - 1);
                    }
                }
                EcrireLigne(" ✗ Format invalide. Ligne A-H, Colonne 1-9.", ConsoleColor.Red);
            }
        }

        public static bool DemanderDirection()
        {
            while (true)
            {
                Ecrire(" Direction (H)orizontale / (V)erticale : ", COULEUR_INFO);
                string s = Console.ReadLine()?.Trim().ToUpper();
                if (s == "H") return true;
                if (s == "V") return false;
                EcrireLigne(" ✗ Entrez H ou V.", ConsoleColor.Red);
            }
        }

        // ── AFFICHAGE ─────────────────────────────────────────────────────

        public static void AfficherBanniere()
        {
            EcrireLigne(@"
  ██████╗  █████╗ ████████╗ █████╗ ██╗██╗     ██╗     ███████╗
  ██╔══██╗██╔══██╗╚══██╔══╝██╔══██╗██║██║     ██║     ██╔════╝
  ██████╔╝███████║   ██║   ███████║██║██║     ██║     █████╗  
  ██╔══██╗██╔══██║   ██║   ██╔══██║██║██║     ██║     ██╔══╝  
  ██████╔╝██║  ██║   ██║   ██║  ██║██║███████╗███████╗███████╗
  ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚══════╝
  ███╗   ██╗ █████╗ ██╗   ██╗ █████╗ ██╗     ███████╗
  ████╗  ██║██╔══██╗██║   ██║██╔══██╗██║     ██╔════╝
  ██╔██╗ ██║███████║██║   ██║███████║██║     █████╗  
  ██║╚██╗██║██╔══██║╚██╗ ██╔╝██╔══██║██║     ██╔══╝  
  ██║ ╚████║██║  ██║ ╚████╔╝ ██║  ██║███████╗███████╗
  ╚═╝  ╚═══╝╚═╝  ╚═╝  ╚═══╝  ╚═╝  ╚═╝╚══════╝╚══════╝⚓", COULEUR_TITRE);
        }

        public static void AfficherDeuxGrilles(Joueur tireur, Joueur cible)
        {
            Console.WriteLine();
            EcrireLigne($"  Votre grille ({tireur.Nom})" +
                        new string(' ', 20) +
                        $"  Tirs sur {cible.Nom}", COULEUR_TITRE);

            // En-têtes colonnes
            string header = "    1  2  3  4  5  6  7  8  9";
            Console.ForegroundColor = COULEUR_TITRE;
            Console.WriteLine(header + "      " + header);
            Console.ResetColor();

            for (int l = 0; l < 8; l++)
            {
                Console.ForegroundColor = COULEUR_TITRE;
                Console.Write($"  {(char)('A' + l)} ");
                Console.ResetColor();

                for (int c = 0; c < 9; c++)
                    AfficherCase(tireur.Grille[l, c], true);

                Console.Write("      ");
                Console.ForegroundColor = COULEUR_TITRE;
                Console.Write($"  {(char)('A' + l)} ");
                Console.ResetColor();

                for (int c = 0; c < 9; c++)
                    AfficherCase(tireur.GrilleAdversaire[l, c], false);

                Console.WriteLine();
            }

            Console.WriteLine();
            AfficherLegende();
        }

        public static void AfficherGrille(char[,] grille, string titre, bool visible)
        {
            EcrireLigne($"\n  {titre}", COULEUR_TITRE);
            Console.ForegroundColor = COULEUR_TITRE;
            Console.WriteLine("    1  2  3  4  5  6  7  8  9");
            Console.ResetColor();
            for (int l = 0; l < 8; l++)
            {
                Console.ForegroundColor = COULEUR_TITRE;
                Console.Write($"  {(char)('A' + l)} ");
                Console.ResetColor();
                for (int c = 0; c < 9; c++)
                    AfficherCase(grille[l, c], visible);
                Console.WriteLine();
            }
        }

        public static void AfficherCase(char etat, bool visible)
        {
            switch (etat)
            {
                case 'B':
                    Console.ForegroundColor = visible ? COULEUR_BATEAU : COULEUR_EAU;
                    Console.Write(visible ? " ■ " : " ~ ");
                    break;
                case 'X':
                    Console.ForegroundColor = COULEUR_TOUCHE;
                    Console.Write(" X ");
                    break;
                case 'O':
                    Console.ForegroundColor = COULEUR_RATE;
                    Console.Write(" ○ ");
                    break;
                default:
                    Console.ForegroundColor = COULEUR_EAU;
                    Console.Write(" ~ ");
                    break;
            }
            Console.ResetColor();
        }

        public static void AfficherLegende()
        {
            Console.ForegroundColor = COULEUR_BATEAU; Console.Write(" ■ ");
            Console.ResetColor(); Console.Write("Bateau  ");
            Console.ForegroundColor = COULEUR_TOUCHE; Console.Write(" X ");
            Console.ResetColor(); Console.Write("Touché  ");
            Console.ForegroundColor = COULEUR_RATE; Console.Write(" ○ ");
            Console.ResetColor(); Console.Write("À l'eau  ");
            Console.ForegroundColor = COULEUR_EAU; Console.Write(" ~ ");
            Console.ResetColor(); Console.WriteLine("Mer");
        }

        public static void AnnoncerVainqueur(Joueur vainqueur)
        {
            Console.Clear();
            AfficherBanniere();
            Console.WriteLine();
            EcrireLigne("╔══════════════════════════════════════╗", COULEUR_SUCCES);
            EcrireLigne($"║  🏆  {vainqueur.Nom,30}  gagne !  ║", COULEUR_SUCCES);
            EcrireLigne("╚══════════════════════════════════════╝", COULEUR_SUCCES);
            Console.WriteLine();
        }

        // ── UTILITAIRES CONSOLE ───────────────────────────────────────────

        public static void Ecrire(string msg, ConsoleColor couleur)
        {
            Console.ForegroundColor = couleur;
            Console.Write(msg);
            Console.ResetColor();
        }

        public static void EcrireLigne(string msg, ConsoleColor couleur)
        {
            Console.ForegroundColor = couleur;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Pause()
        {
            Ecrire("\n Appuyez sur [Entrée] pour continuer...", COULEUR_INFO);
            Console.ReadLine();
        }

        // Palette console
        const ConsoleColor COULEUR_EAU = ConsoleColor.Cyan;
        const ConsoleColor COULEUR_BATEAU = ConsoleColor.DarkGray;
        const ConsoleColor COULEUR_TOUCHE = ConsoleColor.Red;
        const ConsoleColor COULEUR_RATE = ConsoleColor.Blue;
        const ConsoleColor COULEUR_TITRE = ConsoleColor.Yellow;
        const ConsoleColor COULEUR_INFO = ConsoleColor.White;
        const ConsoleColor COULEUR_SUCCES = ConsoleColor.Green;
    }
}
