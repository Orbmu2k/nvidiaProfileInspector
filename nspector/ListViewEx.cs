using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace nspector
{
	internal delegate void DropFilesNativeHandler(string[] files);

	internal class ListViewEx : ListView
	{

		public event DropFilesNativeHandler OnDropFilesNative;

		public event EventHandler<GroupStateChangedEventArgs> GroupStateChanged;

		[DllImport("user32.dll")]
		private	static extern IntPtr SendMessage(IntPtr hWnd, int msg,	IntPtr wPar, IntPtr	lPar);

		private const int LVM_FIRST					= 0x1000;
		private const int LVM_GETCOLUMNORDERARRAY	= (LVM_FIRST + 59);

		private const int WM_PAINT = 0x000F;
		private const int WM_VSCROLL = 0x0115;
		private const int WM_HSCROLL = 0x0114;
		private const int WM_MOUSEWHEEL = 0x020A;

		private struct EmbeddedControl
		{
			internal Control Control;
			internal int Column;
			internal int Row;
			internal DockStyle Dock;
			internal ListViewItem Item;
		}

		private ArrayList _embeddedControls = new ArrayList();

		public ListViewEx()
		{
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.EnableNotifyMessage, true);
		}

		protected override void OnNotifyMessage(Message m)
		{
			if (m.Msg != 0x14)
			{
				base.OnNotifyMessage(m);
			}
		}

		protected int[] GetColumnOrder()
		{
			IntPtr lPar	= Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);

			IntPtr res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lPar);
			if (res.ToInt32() == 0)
			{
				Marshal.FreeHGlobal(lPar);
				return null;
			}

			int	[] order = new int[Columns.Count];
			Marshal.Copy(lPar, order, 0, Columns.Count);

			Marshal.FreeHGlobal(lPar);

			return order;
		}

		protected Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
		{
			Rectangle subItemRect = Rectangle.Empty;

			if (Item == null)
				throw new ArgumentNullException("Item");

			int[] order = GetColumnOrder();
			if (order == null) // No Columns
				return subItemRect;

			if (SubItem >= order.Length)
				throw new IndexOutOfRangeException("SubItem "+SubItem+" out of range");

			Rectangle lviBounds;
			try
			{
				lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
			}
			catch { return subItemRect; }

			int subItemX = lviBounds.Left;

			ColumnHeader col;
			int i;
			for (i=0; i<order.Length; i++)
			{
				col = this.Columns[order[i]];
				if (col.Index == SubItem)
					break;
				subItemX += col.Width;
			}

			subItemRect	= new Rectangle(subItemX, lviBounds.Top-1, this.Columns[order[i]].Width, lviBounds.Height);

			return subItemRect;
		}

		internal void AddEmbeddedControl(Control c, int col, int row)
		{
			AddEmbeddedControl(c,col,row,DockStyle.Fill);
		}

		internal void AddEmbeddedControl(Control c, int col, int row, DockStyle dock)
		{
			if (c==null)
				throw new ArgumentNullException();
			if (col>=Columns.Count || row>=Items.Count)
				throw new ArgumentOutOfRangeException();

			EmbeddedControl ec;
			ec.Control = c;
			ec.Column = col;
			ec.Row = row;
			ec.Dock = dock;
			ec.Item = Items[row];

			_embeddedControls.Add(ec);

			c.Click += new EventHandler(_embeddedControl_Click);

			this.Controls.Add(c);
		}

		internal void RemoveEmbeddedControl(Control c)
		{
			if (c == null)
				throw new ArgumentNullException();

			for (int i=0; i<_embeddedControls.Count; i++)
			{
				EmbeddedControl ec = (EmbeddedControl)_embeddedControls[i];
				if (ec.Control == c)
				{
					c.Click -= new EventHandler(_embeddedControl_Click);
					this.Controls.Remove(c);
					_embeddedControls.RemoveAt(i);
					return;
				}
			}
		}

		internal Control GetEmbeddedControl(int col, int row)
		{
			foreach (EmbeddedControl ec in _embeddedControls)
				if (ec.Row == row && ec.Column == col)
					return ec.Control;

			return null;
		}

		[DefaultValue(View.LargeIcon)]
		internal new View View
		{
			get
			{
				return base.View;
			}
			set
			{
				foreach (EmbeddedControl ec in _embeddedControls)
					ec.Control.Visible = (value == View.Details);

				base.View = value;
			}
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder lpszFile, int cch);
		private const int WM_DROPFILES = 0x233;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_LBUTTONUP)
			{
				base.DefWndProc(ref m); // Fix for collapsible buttons
				return;
			}

			switch (m.Msg)
			{
				case WM_PAINT:
					if (View != View.Details)
						break;

					foreach (EmbeddedControl ec in _embeddedControls)
					{
						// Skip repositioning if the control is a dropped-down ComboBox, prevents it from immediately closing on first click
						if (ec.Control is ComboBox comboBox && comboBox.DroppedDown)
							continue;

						Rectangle rc = this.GetSubItemBounds(ec.Item, ec.Column);

						if ((this.HeaderStyle != ColumnHeaderStyle.None) &&
							(rc.Top<this.Font.Height))
						{
							ec.Control.Visible = false;
							continue;
						}
						else
						{
							ec.Control.Visible = true;
						}

						switch (ec.Dock)
						{
							case DockStyle.Fill:
								break;
							case DockStyle.Top:
								rc.Height = ec.Control.Height;
								break;
							case DockStyle.Left:
								rc.Width = ec.Control.Width;
								break;
							case DockStyle.Bottom:
								rc.Offset(0, rc.Height-ec.Control.Height);
								rc.Height = ec.Control.Height;
								break;
							case DockStyle.Right:
								rc.Offset(rc.Width-ec.Control.Width, 0);
								rc.Width = ec.Control.Width;
								break;
							case DockStyle.None:
								rc.Size = ec.Control.Size;
								break;
						}


						rc.X = rc.X + ec.Control.Margin.Left;
						rc.Y = rc.Y + ec.Control.Margin.Top;
						rc.Width = rc.Width - ec.Control.Margin.Right;
						rc.Height = rc.Height - ec.Control.Margin.Bottom;

						ec.Control.Bounds = rc;

					}
					break;

				case WM_VSCROLL:
				case WM_HSCROLL:
				case WM_MOUSEWHEEL:
					// Close any opened comboboxes if listview is being scrolled
					foreach (EmbeddedControl ec in _embeddedControls)
					{
						if (ec.Control is ComboBox comboBox && comboBox.DroppedDown)
						{
							comboBox.DroppedDown = false;
						}
					}
					break;

				case WM_DROPFILES:

					if (OnDropFilesNative != null)
					{
						var dropped = DragQueryFile(m.WParam, 0xFFFFFFFF, null, 0);
						if (dropped > 0)
						{
							var files = new List<string>();

							for (uint i = 0; i < dropped; i++)
							{
								var size = DragQueryFile(m.WParam, i, null, 0);
								if (size > 0)
								{
									var sb = new StringBuilder(size + 1);
									var result = DragQueryFile(m.WParam, i, sb, size + 1);
									files.Add(sb.ToString());
								}
							}

							OnDropFilesNative(files.ToArray());
						}
					}

					base.WndProc(ref m);
					break;

				case WM_NOTIFY:
				case WM_REFLECT_NOTIFY:
					var nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));

					// Check if this is an (undocumented) listview group notification
					// https://www.zabkat.com/blog/05Feb12-collapsible-listview.htm
					if (nmhdr.code == LVN_GROUPINFO && !_isUpdatingGroups)
					{
						// Group state has changed - get the group info
						var lvGroupInfo = (NMLVGROUP)Marshal.PtrToStructure(m.LParam, typeof(NMLVGROUP));

						// Find the corresponding ListViewGroup
						ListViewGroup changedGroup = null;
						foreach (ListViewGroup group in this.Groups)
						{
							int? groupId = GetGroupID(group);
							if (groupId.HasValue && groupId.Value == lvGroupInfo.iGroupId)
							{
								changedGroup = group;
								break;
							}
						}

						if (changedGroup != null)
						{
							// Determine if collapsed or expanded based on state
							bool isCollapsed = (lvGroupInfo.uNewState & (int)ListViewGroupState.Collapsed) != 0;

							// Fire the event
							GroupStateChanged?.Invoke(this, new GroupStateChangedEventArgs
							{
								Group = changedGroup,
								IsCollapsed = isCollapsed,
								NewState = (ListViewGroupState)lvGroupInfo.uNewState
							});
						}
					}

					break;
			}
			base.WndProc(ref m);
		}

		private void _embeddedControl_Click(object sender, EventArgs e)
		{
			foreach (EmbeddedControl ec in _embeddedControls)
			{
				if (ec.Control == (Control)sender)
				{
					this.SelectedItems.Clear();
					ec.Item.Selected = true;
				}
			}
		}

		// Collapsible groups - https://www.codeproject.com/Articles/451742/Extending-Csharp-ListView-with-Collapsible-Groups

		private bool _isUpdatingGroups = false;

		private const int WM_NOTIFY = 0x004E;
		private const int WM_REFLECT_NOTIFY = 0x204E;

		private const int LVN_FIRST = -100;
		private const int LVN_GROUPINFO = (LVN_FIRST - 88);

		private const int LVM_SETGROUPINFO = (LVM_FIRST + 147);  // ListView messages Setinfo on Group
		private const int WM_LBUTTONUP = 0x0202;				 // Windows message left button

		private delegate void CallBackSetGroupState(ListViewGroup lstvwgrp, ListViewGroupState state);
		private delegate void CallbackSetGroupString(ListViewGroup lstvwgrp, string value);

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVGROUP lParam);

		private static int? GetGroupID(ListViewGroup lstvwgrp)
		{
			int? rtnval = null;
			Type GrpTp = lstvwgrp.GetType();
			if (GrpTp != null)
			{
				PropertyInfo pi = GrpTp.GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
				if (pi != null)
				{
					object tmprtnval = pi.GetValue(lstvwgrp, null);
					if (tmprtnval != null)
					{
						rtnval = tmprtnval as int?;
					}
				}
			}
			return rtnval;
		}

		private static void setGrpState(ListViewGroup lstvwgrp, ListViewGroupState state)
		{
			if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows collaps of ListViewGroups
				return;
			if (lstvwgrp == null || lstvwgrp.ListView == null)
				return;
			if (lstvwgrp.ListView.InvokeRequired)
				lstvwgrp.ListView.Invoke(new CallBackSetGroupState(setGrpState), lstvwgrp, state);
			else
			{
				int? GrpId = GetGroupID(lstvwgrp);
				int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
				LVGROUP group = new LVGROUP();
				group.CbSize = Marshal.SizeOf(group);
				group.State = state;
				group.Mask = ListViewGroupMask.State;
				if (GrpId != null)
				{
					group.IGroupId = GrpId.Value;
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, group);
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, group);
				}
				else
				{
					group.IGroupId = gIndex;
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
				}
				lstvwgrp.ListView.Refresh();
			}
		}

		private static void setGrpFooter(ListViewGroup lstvwgrp, string footer)
		{
			if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows footer on ListViewGroups
				return;
			if (lstvwgrp == null || lstvwgrp.ListView == null)
				return;
			if (lstvwgrp.ListView.InvokeRequired)
				lstvwgrp.ListView.Invoke(new CallbackSetGroupString(setGrpFooter), lstvwgrp, footer);
			else
			{
				int? GrpId = GetGroupID(lstvwgrp);
				int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
				LVGROUP group = new LVGROUP();
				group.CbSize = Marshal.SizeOf(group);
				group.PszFooter = footer;
				group.Mask = ListViewGroupMask.Footer;
				if (GrpId != null)
				{
					group.IGroupId = GrpId.Value;
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, group);
				}
				else
				{
					group.IGroupId = gIndex;
					SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
				}
			}
		}

		public void SetGroupState(ListViewGroupState state)
		{
			_isUpdatingGroups = true;
			foreach (ListViewGroup lvg in this.Groups)
				setGrpState(lvg, state);
			_isUpdatingGroups = false;
		}

		public void SetGroupState(ListViewGroup group, ListViewGroupState state)
		{
			_isUpdatingGroups = true;
			setGrpState(group, state);
			_isUpdatingGroups = false;
		}

		public void SetGroupFooter(ListViewGroup lvg, string footerText)
		{
			setGrpFooter(lvg, footerText);
		}
	}


	/// <summary>
	/// LVGROUP StructureUsed to set and retrieve groups.
	/// </summary>
	/// <example>
	/// LVGROUP myLVGROUP = new LVGROUP();
	/// myLVGROUP.CbSize	// is of managed type uint
	/// myLVGROUP.Mask	// is of managed type uint
	/// myLVGROUP.PszHeader	// is of managed type string
	/// myLVGROUP.CchHeader	// is of managed type int
	/// myLVGROUP.PszFooter	// is of managed type string
	/// myLVGROUP.CchFooter	// is of managed type int
	/// myLVGROUP.IGroupId	// is of managed type int
	/// myLVGROUP.StateMask	// is of managed type uint
	/// myLVGROUP.State	// is of managed type uint
	/// myLVGROUP.UAlign	// is of managed type uint
	/// myLVGROUP.PszSubtitle	// is of managed type IntPtr
	/// myLVGROUP.CchSubtitle	// is of managed type uint
	/// myLVGROUP.PszTask	// is of managed type string
	/// myLVGROUP.CchTask	// is of managed type uint
	/// myLVGROUP.PszDescriptionTop	// is of managed type string
	/// myLVGROUP.CchDescriptionTop	// is of managed type uint
	/// myLVGROUP.PszDescriptionBottom	// is of managed type string
	/// myLVGROUP.CchDescriptionBottom	// is of managed type uint
	/// myLVGROUP.ITitleImage	// is of managed type int
	/// myLVGROUP.IExtendedImage	// is of managed type int
	/// myLVGROUP.IFirstItem	// is of managed type int
	/// myLVGROUP.CItems	// is of managed type IntPtr
	/// myLVGROUP.PszSubsetTitle	// is of managed type IntPtr
	/// myLVGROUP.CchSubsetTitle	// is of managed type IntPtr
	/// </example>
	/// <remarks>
	/// The LVGROUP structure was created by Paw Jershauge
	/// Created: Jan. 2008.
	/// The LVGROUP structure code is based on information from Microsoft's MSDN2 website.
	/// The structure is generated via an automated converter and is as is.
	/// The structure may or may not hold errors inside the code, so use at own risk.
	/// Reference url: http://msdn.microsoft.com/en-us/library/bb774769(VS.85).aspx
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), Description("LVGROUP StructureUsed to set and retrieve groups.")]
	public struct LVGROUP
	{
		/// <summary>
		/// Size of this structure, in bytes.
		/// </summary>
		[Description("Size of this structure, in bytes.")]
		public int CbSize;

		/// <summary>
		/// Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONENo other items are valid.
		/// </summary>
		[Description("Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONE No other items are valid.")]
		public ListViewGroupMask Mask;

		/// <summary>
		/// Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.
		/// </summary>
		[Description("Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.")]
		[MarshalAs(UnmanagedType.LPWStr)]
		public string PszHeader;

		/// <summary>
		/// Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.")]
		public int CchHeader;

		/// <summary>
		/// Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.
		/// </summary>
		[Description("Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.")]
		[MarshalAs(UnmanagedType.LPWStr)]
		public string PszFooter;

		/// <summary>
		/// Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.")]
		public int CchFooter;

		/// <summary>
		/// ID of the group.
		/// </summary>
		[Description("ID of the group.")]
		public int IGroupId;

		/// <summary>
		/// Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.
		/// </summary>
		[Description("Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.")]
		public int StateMask;

		/// <summary>
		/// Flag that can have one of the following values:LVGS_NORMALGroups are expanded, the group name is displayed, and all items in the group are displayed.
		/// </summary>
		[Description("Flag that can have one of the following values:LVGS_NORMAL Groups are expanded, the group name is displayed, and all items in the group are displayed.")]
		public ListViewGroupState State;

		/// <summary>
		/// Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.
		/// </summary>
		[Description("Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.")]
		public uint UAlign;

		/// <summary>
		/// Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.
		/// </summary>
		[Description("Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.")]
		public IntPtr PszSubtitle;

		/// <summary>
		/// Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.")]
		public uint CchSubtitle;

		/// <summary>
		/// Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.
		/// </summary>
		[Description("Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.")]
		[MarshalAs(UnmanagedType.LPWStr)]
		public string PszTask;

		/// <summary>
		/// Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.")]
		public uint CchTask;

		/// <summary>
		/// Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
		/// </summary>
		[Description("Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
		[MarshalAs(UnmanagedType.LPWStr)]
		public string PszDescriptionTop;

		/// <summary>
		/// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.")]
		public uint CchDescriptionTop;

		/// <summary>
		/// Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
		/// </summary>
		[Description("Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
		[MarshalAs(UnmanagedType.LPWStr)]
		public string PszDescriptionBottom;

		/// <summary>
		/// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.")]
		public uint CchDescriptionBottom;

		/// <summary>
		/// Windows Vista. Index of the title image in the control imagelist.
		/// </summary>
		[Description("Windows Vista. Index of the title image in the control imagelist.")]
		public int ITitleImage;

		/// <summary>
		/// Windows Vista. Index of the extended image in the control imagelist.
		/// </summary>
		[Description("Windows Vista. Index of the extended image in the control imagelist.")]
		public int IExtendedImage;

		/// <summary>
		/// Windows Vista. Read-only.
		/// </summary>
		[Description("Windows Vista. Read-only.")]
		public int IFirstItem;

		/// <summary>
		/// Windows Vista. Read-only in non-owner data mode.
		/// </summary>
		[Description("Windows Vista. Read-only in non-owner data mode.")]
		public IntPtr CItems;

		/// <summary>
		/// Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.
		/// </summary>
		[Description("Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.")]
		public IntPtr PszSubsetTitle;

		/// <summary>
		/// Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.
		/// </summary>
		[Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.")]
		public IntPtr CchSubsetTitle;
	}

	public class GroupStateChangedEventArgs : EventArgs
	{
		public ListViewGroup Group { get; set; }
		public bool IsCollapsed { get; set; }
		public ListViewGroupState NewState { get; set; }
	}

	public enum ListViewGroupMask
	{
		None = 0x00000,
		Header = 0x00001,
		Footer = 0x00002,
		State = 0x00004,
		Align = 0x00008,
		GroupId = 0x00010,
		SubTitle = 0x00100,
		Task = 0x00200,
		DescriptionTop = 0x00400,
		DescriptionBottom = 0x00800,
		TitleImage = 0x01000,
		ExtendedImage = 0x02000,
		Items = 0x04000,
		Subset = 0x08000,
		SubsetItems = 0x10000
	}

	public enum ListViewGroupState
	{
		/// <summary>
		/// Groups are expanded, the group name is displayed, and all items in the group are displayed.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// The group is collapsed.
		/// </summary>
		Collapsed = 1,
		/// <summary>
		/// The group is hidden.
		/// </summary>
		Hidden = 2,
		/// <summary>
		/// Version 6.00 and Windows Vista. The group does not display a header.
		/// </summary>
		NoHeader = 4,
		/// <summary>
		/// Version 6.00 and Windows Vista. The group can be collapsed.
		/// </summary>
		Collapsible = 8,
		/// <summary>
		/// Version 6.00 and Windows Vista. The group has keyboard focus.
		/// </summary>
		Focused = 16,
		/// <summary>
		/// Version 6.00 and Windows Vista. The group is selected.
		/// </summary>
		Selected = 32,
		/// <summary>
		/// Version 6.00 and Windows Vista. The group displays only a portion of its items.
		/// </summary>
		SubSeted = 64,
		/// <summary>
		/// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
		/// </summary>
		SubSetLinkFocused = 128,
	}

	// Required structures for the notification
	[StructLayout(LayoutKind.Sequential)]
	public struct NMHDR
	{
		public IntPtr hwndFrom;
		public UIntPtr idFrom;
		public int code;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVGROUP
	{
		public NMHDR hdr;
		public int iGroupId;
		public uint uNewState;
		public uint uOldState;
		public int state;  // Current state
	}
}
