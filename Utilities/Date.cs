using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace joc_cu_romani_si_barbari.Utilities
{
    class Date
    {
        private int day, month, year;
        public Date(int d, int m, int y) {
            day = d;
            month = m;
            year = y;
        }
        /// <summary>
        /// Does all the neccessary computations to properly advance the date 1 day
        /// </summary>
        public void next() {
            day++;
            if(day == 32){
                day = 1;
                month++;
                if(month == 12){
                    month = 1;
                    year++;
                }
            }
            if( day == 31 && (month == 4 || month == 6 || month == 9 || month == 11)){
                day = 1;
                month ++;
            }
            if(day == 29 && month == 2){
                day = 1;
                month++;
            }
        }
    
        public bool equals(Date other){
            return (other.day == day) && (other.month == month) && (other.year == year);
        }
    
        public String ToString(){
            if(month < 10)
                return day+".0"+month+"."+year;
            else
                return day+"."+month+"."+year;
        }
    }
}
