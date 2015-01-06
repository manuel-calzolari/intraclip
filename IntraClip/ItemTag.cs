/*
 * IntraClip - A simple clipboard manager for Windows
 * Copyright (C) 2011  Manuel Calzolari
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, version 3 of the License.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace IntraClip
{
    public class ItemTag
    {
        private string type;

        private object data;

        public ItemTag() { }

        public ItemTag(string type, object data)
        {
            this.type = type;
            this.data = data;
        }

        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
    }
}
