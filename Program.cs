﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
                Match m = Regex.Match(line, @"(.+):([\w\-\./]*),([0-9\-]*),*(.+)*");
                
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
                    repo_store[intID].dateCreated = Convert.ToDateTime(date);
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
                Match mtch = Regex.Match(line, @"(.+):([\w0-9\-\.\+\#\s]*;[0-9]*,*)+");               
                MatchCollection mColl = Regex.Matches(line, @"([\w0-9\-\.\+\#\s]*;[0-9]*)");

                string id = mtch.Groups[1].Value;
                foreach (Match m in mColl)
                {
                    string lang_count_pair = m.Groups[1].Value;
                    Match lang_count = Regex.Match(lang_count_pair, "(.+);(.+)");
                    string lang = lang_count.Groups[1].Value;
                    string count = lang_count.Groups[2].Value;
                    LanguageStat language = new LanguageStat(lang, Convert.ToInt32(count));
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
            public int pop;

            public PopCount(int r, int p)
            {
                repo = r;
                pop = p;
            }
        }

        public static string predict(int user)
        {
            string output = user + ":";
            int num = 0;
            List<int> parents_choosen = new List<int>();
            List<int> parents_parents_choosen = new List<int>();


            // follow parents
            foreach (int x in user_store[user].following)
            {
                if (repo_store[x].parent != -1 && !user_store[user].following.Contains(repo_store[x].parent) && 
                    !parents_choosen.Contains(repo_store[x].parent))
                {
                    num++;
                    parents_choosen.Add(repo_store[x].parent);
                    output += repo_store[x].parent;
                    if (num < 10)
                        output += ",";
                    if (num == 8)
                    {
                        break;
                    }
                    if (num == 10)
                    {
                        return output;
                    }
                }
            }

            // follow parents of parents
            foreach (int x in parents_choosen)
            {
                if (repo_store[x].parent != -1 && !user_store[user].following.Contains(repo_store[x].parent) &&
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
                }
            }
            
            // follow popular amongst followers of followed ??? !
            PopCount[] pops = new PopCount[repo_store.Length];
            for(int q=0; q<pops.Length; q++)
            {
                pops[q] = new PopCount(q, 0);
            }
            for (int w = 0; w < user_store[user].following.Count; w++)
            {
                if (repo_store[user_store[user].following[w]].followers.Count > 15)
                    continue;
                for (int x = 0; x < repo_store[user_store[user].following[w]].followers.Count; x++)
                {
                    Person peep = user_store[ repo_store[user_store[user].following[w]].followers[x] ];
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
                if (!user_store[user].following.Contains(popp.repo))
                {
                    num++;
                    output += popp.repo;
                    if (num < 10)
                        output += ",";
                    if (num == 10)
                        return output;
                }
            }


      
            // follow popular
            foreach( Repo r in popular_repos)
            {
                if (!user_store[user].following.Contains(r.id))
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


        static void Main(string[] args)
        {
            for (int i = 0; i < 999999; i++)
                user_store[i] = new Person(i);

            loadRepos();
            loadLangs();
            loadFollows();
            repo_store[0] = new Repo();
            
            //
            // ORDER BY THE MOST POPULAR REPOS
            //
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
