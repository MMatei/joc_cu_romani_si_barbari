using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace joc_cu_romani_si_barbari.Utilities
{
    class MusicPlayer
    {
        private List<Song> music;
        private int crrtSong = 0;
        private Random rand = new Random();

        public MusicPlayer(List<Song> _music)
        {
            music = new List<Song>();
            // shuffle da playlist
            while (_music.Count != 0)
            {
                int index = rand.Next(_music.Count);
                music.Add(_music[index]);
                _music.RemoveAt(index);
            }
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(music[0]);
        }

        public float getVolume()
        {
            return MediaPlayer.Volume;
        }

        public void setVolume(float newVolume)
        {
            MediaPlayer.Volume = newVolume;
        }

        /// <summary>
        /// plays next song if the current one has finished playing and re-shuffles the playlist, if necessary
        /// </summary>
        public void wazzap()
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {// the song has finished playing
                crrtSong++;
                if (crrtSong > music.Count)
                {// am ajuns la finalul playlist-ului => shuffle
                    List<Song> _music = new List<Song>();
                    while (music.Count != 0)
                    {
                        int index = rand.Next(music.Count);
                        _music.Add(music[index]);
                        music.RemoveAt(index);
                    }
                    music = _music;
                    crrtSong = 0;
                }
                MediaPlayer.Play(music[crrtSong]);
            }
        }
    }
}
