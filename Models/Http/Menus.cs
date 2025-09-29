
namespace MeowMemoirsAPI.Models.Http
{
    /// <summary>
    /// 菜单信息
    /// </summary>
    public class Menus
    {
        /// <summary>
        /// 通知编号
        /// </summary>
        public required List<MenuItem> MenusInfos { get; set; }
    }
    /// <summary>
    /// 菜单信息
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string? Name { get; set; }//路由唯一名称（用于编程式导航，对应 Vue Router 的 name）	                "svgIconPage"
        /// <summary>
        /// 菜单地址
        /// </summary>
        public string? Path { get; set; } //前端路由路径（对应 Vue Router 的 path）	                        "/module/svgIcon"  
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string? Icon { get; set; }//图标标识（可能对应 Element-UI/Ant Design 的图标组件名或自定义图标库）	    "ReadingLamp"
        /// <summary>
        /// 菜单ID
        /// </summary>
        public int MenuId { get; set; }        //菜单唯一标识符，用于区分不同菜单项	                            85
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string? MenuName { get; set; }        //菜单显示的中文名称（面向用户）	                                "SVG 图标"
        /// <summary>
        /// 菜单英文名称
        /// </summary>
        public string? EnName { get; set; }         //菜单的英文名称（可能用于国际化或多语言场景）	                    "SVG Icon"

        /// <summary>
        /// 父级菜单的 ID
        /// </summary>
        public int ParentId { get; set; }        //父级菜单的 ID（用于构建层级结构，0 表示根节点）	                8
        /// <summary>
        /// 菜单类型
        /// </summary>
        public string? MenuType { get; set; }        //菜单类型（通常用于区分目录、页面、按钮等，需结合业务约定）	        "1"=目录, "2"=页面     
        /// <summary>
        /// 组件路径
        /// </summary>
        public string? Component { get; set; }       //组件路径（动态导入时的文件路径，如 @/views/module/svgIcon/index.vue）	    "module/svgIcon/index"
        /// <summary>
        /// 是否隐藏菜单
        /// </summary>
        public string? IsHide { get; set; }      //是否隐藏菜单（"1"=隐藏，"0"=显示；隐藏后可能仍可通过路径访问，需结合权限控制）	"1"
        /// <summary>
        /// 是否是外部链接
        /// </summary>
        public string? IsLink { get; set; }          //是否是外部链接（非空时表示跳转到外部 URL，如 "https://example.com"）	            ""
        /// <summary>
        /// 是否缓存组件
        /// </summary>
        public string? IsKeepAlive { get; set; } //是否缓存组件（"1"=启用 keep-alive 缓存，"0"=不缓存）	                     "0"

        /// <summary>
        /// 是否全屏显示
        /// </summary>
        public string? IsFull { get; set; }          //是否全屏显示（"1"=隐藏侧边栏/导航栏，"0"=正常布局）	                    "1"

        /// <summary>
        /// 是否固定标签页
        /// </summary>
        public string? IsAffix { get; set; }     //是否固定标签页（"1"=在标签栏中不可关闭，"0"=可关闭）	                    "1"
        /// <summary>
        /// 重定向路径
        /// </summary>
        public string? Redirect { get; set; }	    //重定向路径（为空时无重定向，常用于父级目录跳转子页面）	                     ""
        /// <summary>
        /// 二级菜单
        /// </summary>
        public List<MenuItem>? Children { get; set; }

    }
}
