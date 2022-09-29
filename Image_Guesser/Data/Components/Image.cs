using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Image_Guesser.Data.Components
{
    public class Image
    {
        private String correctName;
        private String imageUrl;
        private int blurValue;
        private const String imgFolder = "object_images_A-C";
        public Image(String[] directories, int startingTime)
        {
            var rand = new Random();
            int randIndex = rand.Next(directories.Length);
            //this is to iterate through the path directory to find where the name of the image
            //starts
            int startIndex = -1;
            
            for(int i = 0; i< directories[randIndex].Length-imgFolder.Length; i++)
            {
                if(directories[randIndex].Substring(i, imgFolder.Length).Equals(imgFolder))
                {
                    startIndex = i+imgFolder.Length+1;
                }
            }
            correctName = directories[randIndex].Substring(startIndex);
            imageUrl = directories[randIndex].Substring(startIndex-(imgFolder.Length+1))+"\\" + correctName+"_0" + rand.Next(3,10)+ "s.jpg";
            correctName = correctName.Replace('_', ' ');
            // using 10 seconds as startingTime
            // should scale blur based on image size
            this.blurValue = startingTime;
        }

        public String getImageUrl()
        {
            return imageUrl;
        }

        public String getCorrectName()
        {
            return correctName;
        }

        public int getBlurValue()
        {
            return blurValue;
        }

        public void decreaseBlur(int timeLeft)
        {
            // should scale blur based on image size and timeLeft
            blurValue = timeLeft;
        }

    }
}
