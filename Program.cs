/// HEADER here

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace ghcontest
{
    class Program
    {

        class LanguageStat
        {
            public string language_name;
            public int lines;

            public LanguageStat(string name, int l)
            {
                language_name = name;
                lines = l;
            }
        }

        class Repo
        {
            public int id;
            public List<int> followers;
            public int parent;
            public DateTime dateCreated;
            public string desc;
            public List<LanguageStat> languages;
            public string author;

            public Repo()
            {
                followers = new List<int>();
                languages = new List<LanguageStat>();
            }

            public bool containsLang(string lang)
            {
                foreach (LanguageStat ls in languages)
                {
                    if (ls.language_name == lang)
                        return true;
                }
                return false;
            }
        }

        class Person
        {
            public int id;
            public List<int> following;

            public Person(int i)
            {
                id = i;
                following = new List<int>();
            }
        }

        static Person[] user_store = new Person[999999];
        static Repo[] repo_store = new Repo[123345];
        static Repo[] popular_repos = new Repo[123345];

        static public void loadRepos()
        {
            // load repos
            StreamReader sr = new StreamReader("repos.txt");
            string line = "";
            line = sr.ReadLine();
            while (line != null)
            {
                Match m = Regex.Match(line,
@"(.+):([\w\-\./]*),([0-9\-]*),*(.+)*");

                if (m.Success)
                {
                    string whole_line = m.Groups[0].Value;
                    string id = m.Groups[1].Value;
                    string desc = m.Groups[2].Value;
                    string date = m.Groups[3].Value;
                    string parent = m.Groups[4].Value;

                    int intID = Convert.ToInt32(id);
                    repo_store[intID] = new Repo();
                    repo_store[intID].id = intID;
                    repo_store[intID].desc = desc;
                    repo_store[intID].author = desc.Substring(0, desc.IndexOf('/'));
                    repo_store[intID].dateCreated =
Convert.ToDateTime(date);
                    if (parent != "")
                    {
                        repo_store[intID].parent = Convert.ToInt32(parent);
                    }
                    else
                    {
                        repo_store[intID].parent = -1;
                    }
                }

                line = sr.ReadLine();
            }
            sr.Close();
        }

        public static void loadLangs()
        {
            StreamReader sr = new StreamReader("lang.txt");
            string line = "";
            line = sr.ReadLine();
            while (line != null)
            {
                Match mtch = Regex.Match(line,
@"(.+):([\w0-9\-\.\+\#\s]*;[0-9]*,*)+");
                MatchCollection mColl = Regex.Matches(line,
@"([\w0-9\-\.\+\#\s]*;[0-9]*)");

                string id = mtch.Groups[1].Value;
                foreach (Match m in mColl)
                {
                    string lang_count_pair = m.Groups[1].Value;
                    Match lang_count = Regex.Match(lang_count_pair,
"(.+);(.+)");
                    string lang = lang_count.Groups[1].Value;
                    string count = lang_count.Groups[2].Value;
                    LanguageStat language = new LanguageStat(lang,
Convert.ToInt32(count));
                    try
                    {

                        repo_store[Convert.ToInt32(id)].languages.Add(language);
                    }
                    catch (Exception ex)
                    {
                        // not in repo
                    }
                }

                line = sr.ReadLine();
            }
            sr.Close();
        }

        public static void loadFollows()
        {
            StreamReader sr = new StreamReader("data.txt");
            string line = "";
            line = sr.ReadLine();
            while (line != null)
            {
                Match m = Regex.Match(line, "(.+):(.+)");
                string user = m.Groups[1].Value;
                string repo = m.Groups[2].Value;
                int intUser = Convert.ToInt32(user);
                int intRepo = Convert.ToInt32(repo);

                repo_store[intRepo].followers.Add(intUser);
                user_store[intUser].following.Add(intRepo);

                line = sr.ReadLine();
            }
            sr.Close();
        }

        class PopCount
        {
            public int repo;
            public double pop;

            public PopCount(int r, double p)
            {
                repo = r;
                pop = p;
            }
        }

        static int parent_max = 0;
        static int low_follows = 0;

        public static string predict(int user)
        {
            string output = user + ":";
            int num = 0;
            List<int> parents_choosen = new List<int>();
            List<int> parents_parents_choosen = new List<int>();
            List<int> predicted = new List<int>();

            // follow parents
            foreach (int x in user_store[user].following)
            {

                if (repo_store[x].parent != -1 &&
!user_store[user].following.Contains(repo_store[x].parent) &&
                    !parents_choosen.Contains(repo_store[x].parent))
                {
                    num++;
                    parents_choosen.Add(repo_store[x].parent);
                    output += repo_store[x].parent;
                    predicted.Add(repo_store[x].parent);
                    if (num < 10)
                        output += ",";
                    if (num == 2)
                    {
                        parent_max++;
                        break;
                    }
                    if (num == 10)
                    {
                        return output;
                    }
                }
            }

            // follow parents of parents
            /*
    foreach (int x in parents_choosen)
            {
                if (repo_store[x].parent != -1 &&
!user_store[user].following.Contains(repo_store[x].parent) &&
                    !parents_parents_choosen.Contains(repo_store[x].parent))
                {
                    num++;
                    parents_parents_choosen.Add(repo_store[x].parent);
                    output += repo_store[x].parent;
                    if (num < 10)
                        output += ",";
                    if (num == 10)
                    {
                        return output;
                    }
                    if (num == 8)
                    {
                        break;
                    }
                }
            }
            */

            // follow popular amongst authors other projects
            PopCount[] pops = new PopCount[repo_store.Length];
            for (int p = 0; p < pops.Length; p++)
            {
                pops[p] = new PopCount(p, 0);
            }
            List<string> authors = new List<string>();
            for (int z = 0; z < user_store[user].following.Count; z++)
            {
                authors.Add(repo_store[user_store[user].following[z]].author);
            }
            /*
            for (int f = 0; f < repo_store.Length; f++)
            {
                try
                {
                    if (authors.Contains(repo_store[f].author))
                    {
                        pops[f].pop = repo_store[f].followers.Count;
                    }
                }
                catch (Exception)
                {
                    //empty
                }
            }
            */
            foreach (string a in authors)
            {
                List<Repo> r_list = (List<Repo>)author_set[a];
                foreach (Repo rep in r_list)
                {
                    pops[rep.id].pop = rep.followers.Count;
                }
            }
 

            Array.Sort(pops, delegate(PopCount r1, PopCount r2)
            {
                if (r1 == null && r2 == null)
                    return 0;
                if (r1 == null)
                    return 1;
                if (r2 == null)
                    return -1;
                return r2.pop.CompareTo(r1.pop);
            });
            foreach (PopCount popp in pops)
            {
                if (!user_store[user].following.Contains(popp.repo) &&
 !predicted.Contains(popp.repo))
                {
                    num++;
                    output += popp.repo;
                    if (repo_store[popp.repo].followers.Count < 5)
                    {
                        low_follows++;
                    }
                    predicted.Add(popp.repo);
                    if (num < 10)
                        output += ",";
                    if (num == 7)
                        break;
                    if (num == 10)
                        return output;
                }
            }
            

            // follow popular amongst followers of followed ??? !
            pops = new PopCount[repo_store.Length];
            for (int q = 0; q < pops.Length; q++)
            {
                pops[q] = new PopCount(q, 0);
            }

            for (int w = 0; w < user_store[user].following.Count; w++)
            {
                for (int x = 0; x <
repo_store[user_store[user].following[w]].followers.Count; x++)
                {
                    Person peep = user_store[
repo_store[user_store[user].following[w]].followers[x]];
                    for (int y = 0; y < peep.following.Count; y++)
                    {
                        pops[peep.following[y]].pop++;
                    }
                }
            }

            Array.Sort(pops, delegate(PopCount r1, PopCount r2)
            {
                if (r1 == null && r2 == null)
                    return 0;
                if (r1 == null)
                    return 1;
                if (r2 == null)
                    return -1;
                return r2.pop.CompareTo(r1.pop);
            });

            foreach (PopCount popp in pops)
            {
                if (!user_store[user].following.Contains(popp.repo) &&
!predicted.Contains(popp.repo))
                {
                    num++;
                    output += popp.repo;
                    predicted.Add(popp.repo);
                    if (num < 10)
                        output += ",";
                    if (num == 10)
                        return output;
                }
            }



            // follow popular
            foreach (Repo r in popular_repos)
            {
                if (!user_store[user].following.Contains(r.id) &&
!predicted.Contains(r.id))
                {
                    num++;
                    output += r.id;
                    if (num < 10)
                        output += ",";
                    if (num == 10)
                        break;
                }
            }
            return output;
        }


        public static Hashtable author_set;

        static void Main(string[] args)
        {
            author_set = new Hashtable();

            for (int i = 0; i < 999999; i++)
                user_store[i] = new Person(i);

            System.Console.WriteLine("Loading repos...");
            loadRepos();
            System.Console.WriteLine("Loading langs...");
            loadLangs();
            System.Console.WriteLine("Loading follows...");
            loadFollows();
            repo_store[0] = new Repo();

            //
            // ORDER BY THE MOST POPULAR REPOS
            //
            System.Console.WriteLine("Sorting popular...");
            Array.Copy(repo_store, popular_repos, repo_store.Length);
            Array.Sort(popular_repos, delegate(Repo r1, Repo r2)
            {
                if (r1 == null && r2 == null)
                    return 0;
                if (r1 == null)
                    return 1;
                if (r2 == null)
                    return -1;
                return r2.followers.Count.CompareTo(r1.followers.Count);
            });

            // load up the hastable
            System.Console.WriteLine("Creating Author Hastable...");
            foreach (Repo r in repo_store)
            {
                
                if (r == null || r.author == null)
                    continue;
                List<Repo> r_list = null;
                try
                {
                    r_list = (List<Repo>)author_set[r.author];
                }
                catch (Exception)
                {
                    continue;
                }
                if (r_list == null)
                {
                    r_list = new List<Repo>();
                    r_list.Add(r);
                    author_set.Add(r.author, r_list);
                }
                else
                {
                    r_list.Add(r);
                    author_set[r.author] = r_list;
                }
                

                
            }

            StreamWriter sw = new StreamWriter("results.txt");
            StreamReader sr = new StreamReader("test.txt");

            string line = sr.ReadLine();
            while (line != null)
            {
                string res = predict(Convert.ToInt32(line));
                System.Console.WriteLine(res);
                sw.WriteLine(res);
                line = sr.ReadLine();
            }

            sr.Close();
            sw.Close();

            int q = 0;

        }
    }
}

