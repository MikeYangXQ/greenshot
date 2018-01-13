#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2017 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace GreenshotPlugin.Core.Credentials
{
	/// <summary>http://www.pinvoke.net/default.aspx/Enums.CredUIReturnCodes</summary>
	public enum CredUIReturnCodes
	{
		NO_ERROR = 0,
		ERROR_INVALID_PARAMETER = 87,
		ERROR_INSUFFICIENT_BUFFER = 122,
		ERROR_INVALID_FLAGS = 1004,
		ERROR_NOT_FOUND = 1168,
		ERROR_CANCELLED = 1223,
		ERROR_NO_SUCH_LOGON_SESSION = 1312,
		ERROR_INVALID_ACCOUNT_NAME = 1315
	}
}