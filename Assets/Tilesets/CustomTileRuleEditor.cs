using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

// Add custom rules to the GUI on line 200ish
//private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
namespace UnityEditor
{
    [CustomEditor(typeof(CustomRuleTile), true)]
    [CanEditMultipleObjects]
    internal class RuleTileEditor : Editor
    {
        private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";
        private const string s_OIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAxnpUWHRSYXcgcHJvZmlsZSB0eXBlIGV4aWYAAHjabVDbDcQgDPtnihuBkPAah7ZUug1u/DMkrdqqRjjBQSbE9d93d5+BQOIk5pJqSh6QKjU0JMUr2mTyMlmxW6S77s5CgMSIrMeS7P6h02mgoSGLF6OyWmG5F6qYf3kY2UM8OgpINjOqZsRBC2QGTb/lUy35+oWl+zuKbjdo7dPbk732PEvG9LYIkUPoDBnMLNoAjy2OG5I8ueIicUIesRoLk3WCgbzN6YD7A9UeWkFp8TiyAAABhGlDQ1BJQ0MgcHJvZmlsZQAAeJx9kT1Iw0AcxV9TpSoVBwuKOGSoTu2iUhylikWwUNoKrTqYXPoFTRqSFBdHwbXg4Mdi1cHFWVcHV0EQ/ABxdnBSdJES/5cUWsR4cNyPd/ced+8AoVllqtkzB6iaZaQTcTGXXxUDrwigHyOIISIxU09mFrPwHF/38PH1LsqzvM/9OQaVgskAn0g8x3TDIt4gjm1aOud94hArSwrxOXHEoAsSP3JddvmNc8lhgWeGjGx6njhELJa6WO5iVjZU4hnisKJqlC/kXFY4b3FWq3XWvid/YbCgrWS4TnMcCSwhiRREyKijgiosRGnVSDGRpv24h3/M8afIJZOrAkaOBdSgQnL84H/wu1uzOD3lJgXjQO+LbX9MAIFdoNWw7e9j226dAP5n4Err+GtNYPaT9EZHCx8BQ9vAxXVHk/eAyx1g9EmXDMmR/DSFYhF4P6NvygPDt8DAmttbex+nD0CWulq+AQ4OgckSZa97vLuvu7d/z7T7+wET5nLnDftzqwAADXZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IlhNUCBDb3JlIDQuNC4wLUV4aXYyIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIgogICAgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIKICAgIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIKICAgIHhtbG5zOkdJTVA9Imh0dHA6Ly93d3cuZ2ltcC5vcmcveG1wLyIKICAgIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIgogICAgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIgogICB4bXBNTTpEb2N1bWVudElEPSJnaW1wOmRvY2lkOmdpbXA6NTAwMDUzZmItNmE5ZC00ZTRjLTgyYjYtZWQxZGIwZmJkYjdkIgogICB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjVhMTM1ODY3LWM1OWQtNDFlNy04OThlLTIyNDQzNzZkODJmNSIKICAgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOmE4Y2YzNTYyLThlMjQtNDRlNS04OWI1LTMyMDdhYzY4NjJkYSIKICAgZGM6Rm9ybWF0PSJpbWFnZS9wbmciCiAgIEdJTVA6QVBJPSIyLjAiCiAgIEdJTVA6UGxhdGZvcm09IldpbmRvd3MiCiAgIEdJTVA6VGltZVN0YW1wPSIxNzIwNTY5MzQyMTcxMTc4IgogICBHSU1QOlZlcnNpb249IjIuMTAuMzQiCiAgIHRpZmY6T3JpZW50YXRpb249IjEiCiAgIHhtcDpDcmVhdG9yVG9vbD0iR0lNUCAyLjEwIgogICB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0OjA3OjA5VDE2OjU1OjQxLTA3OjAwIgogICB4bXA6TW9kaWZ5RGF0ZT0iMjAyNDowNzowOVQxNjo1NTo0MS0wNzowMCI+CiAgIDx4bXBNTTpIaXN0b3J5PgogICAgPHJkZjpTZXE+CiAgICAgPHJkZjpsaQogICAgICBzdEV2dDphY3Rpb249InNhdmVkIgogICAgICBzdEV2dDpjaGFuZ2VkPSIvIgogICAgICBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmEwODFhZTY4LWQ2M2EtNDc3MS1iODg3LWU5NzA0YzA1NGM2ZCIKICAgICAgc3RFdnQ6c29mdHdhcmVBZ2VudD0iR2ltcCAyLjEwIChXaW5kb3dzKSIKICAgICAgc3RFdnQ6d2hlbj0iMjAyNC0wNy0wOVQxNjo1NTo0MiIvPgogICAgPC9yZGY6U2VxPgogICA8L3htcE1NOkhpc3Rvcnk+CiAgPC9yZGY6RGVzY3JpcHRpb24+CiA8L3JkZjpSREY+CjwveDp4bXBtZXRhPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgCjw/eHBhY2tldCBlbmQ9InciPz4UFAouAAAABmJLR0QAPQAiACJOWxQiAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAB3RJTUUH6AcJFzcqqz58wgAAAKNJREFUKM+tk80NwyAMhR8oGwTJd/bIXJ6BudjDd0tkBnohkevSNmrzTpal9+E/Aibacu4+V0WCzy0zUyF6ATLQPSRYozWx6hn7/AEI3siq+3ghGXAbkNUCnspm1d2aTL9plN4OAAAsvtwrKkRgoEfXS/pkqiLJziLiD91jLkTnVN9py7nZ+cQqEmwfVzRdVSFaGWjf9nzfhV267QGb3vYvv+oBkMVlxA3njaoAAAAASUVORK5CYII=";
        private const string s_Arrow0 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPzZExDoQwDATzE4oU4QXXcgUFj+YxtETwgpMwXuFcwMFSRMVKKwzZcWzhiMg91jtg34XIntkre5EaT7yjjhI9pOD5Mw5k2X/DdUwFr3cQ7Pu23E/BiwXyWSOxrNqx+ewnsayam5OLBtbOGPUM/r93YZL4/dhpR/amwByGFBz170gNChA6w5bQQMqramBTgJ+Z3A58WuWejPCaHQAAAABJRU5ErkJggg==";
        private const string s_Arrow1 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPxYzBDYAgEATpxYcd+PVr0fZ2siZrjmMhFz6STIiDs8XMlpEyi5RkO/d66TcgJUB43JfNBqRkSEYDnYjhbKD5GIUkDqRDwoH3+NgTAw+bL/aoOP4DOgH+iwECEt+IlFmkzGHlAYKAWF9R8zUnAAAAAElFTkSuQmCC";
        private const string s_Arrow2 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAC0SURBVDhPjVE5EsIwDMxPKFKYF9CagoJH8xhaMskLmEGsjOSRkBzYmU2s9a58TUQUmCH1BWEHweuKP+D8tphrWcAHuIGrjPnPNY8X2+DzEWE+FzrdrkNyg2YGNNfRGlyOaZDJOxBrDhgOowaYW8UW0Vau5ZkFmXbbDr+CzOHKmLinAXMEePyZ9dZkZR+s5QX2O8DY3zZ/sgYcdDqeEVp8516o0QQV1qeMwg6C91toYoLoo+kNt/tpKQEVvFQAAAAASUVORK5CYII=";
        private const string s_Arrow3 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAAB2SURBVDhPzY1LCoAwEEPnLi48gW5d6p31bH5SMhp0Cq0g+CCLxrzRPqMZ2pRqKG4IqzJc7JepTlbRZXYpWTg4RZE1XAso8VHFKNhQuTjKtZvHUNCEMogO4K3BhvMn9wP4EzoPZ3n0AGTW5fiBVzLAAYTP32C2Ay3agtu9V/9PAAAAAElFTkSuQmCC";
        private const string s_Arrow5 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABqSURBVDhPnY3BCYBADASvFx924NevRdvbyoLBmNuDJQMDGjNxAFhK1DyUQ9fvobCdO+j7+sOKj/uSB+xYHZAxl7IR1wNTXJeVcaAVU+614uWfCT9mVUhknMlxDokd15BYsQrJFHeUQ0+MB5ErsPi/6hO1AAAAAElFTkSuQmCC";
        private const string s_Arrow6 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACaSURBVDhPxZExEkAwEEVzE4UiTqClUDi0w2hlOIEZsV82xCZmQuPPfFn8t1mirLWf7S5flQOXjd64vCuEKWTKVt+6AayH3tIa7yLg6Qh2FcKFB72jBgJeziA1CMHzeaNHjkfwnAK86f3KUafU2ClHIJSzs/8HHLv09M3SaMCxS7ljw/IYJWzQABOQZ66x4h614ahTCL/WT7BSO51b5Z5hSx88AAAAAElFTkSuQmCC";
        private const string s_Arrow7 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABQSURBVDhPYxh8QNle/T8U/4MKEQdAmsz2eICx6W530gygr2aQBmSMphkZYxqErAEXxusKfAYQ7XyyNMIAsgEkaYQBkAFkaYQBsjXSGDAwAAD193z4luKPrAAAAABJRU5ErkJggg==";
        private const string s_Arrow8 = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAACYSURBVDhPxZE9DoAwCIW9iUOHegJXHRw8tIdx1egJTMSHAeMPaHSR5KVQ+KCkCRF91mdz4VDEWVzXTBgg5U1N5wahjHzXS3iFFVRxAygNVaZxJ6VHGIl2D6oUXP0ijlJuTp724FnID1Lq7uw2QM5+thoKth0N+GGyA7IA3+yM77Ag1e2zkey5gCdAg/h8csy+/89v7E+YkgUntOWeVt2SfAAAAABJRU5ErkJggg==";
        private const string s_MirrorX = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG1JREFUOE+lj9ENwCAIRB2IFdyRfRiuDSaXAF4MrR9P5eRhHGb2Gxp2oaEjIovTXSrAnPNx6hlgyCZ7o6omOdYOldGIZhAziEmOTSfigLV0RYAB9y9f/7kO8L3WUaQyhCgz0dmCL9CwCw172HgBeyG6oloC8fAAAAAASUVORK5CYII=";
        private const string s_MirrorY = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG9JREFUOE+djckNACEMAykoLdAjHbPyw1IOJ0L7mAejjFlm9hspyd77Kk+kBAjPOXcakJIh6QaKyOE0EB5dSPJAiUmOiL8PMVGxugsP/0OOib8vsY8yYwy6gRyC8CB5QIWgCMKBLgRSkikEUr5h6wOPWfMoCYILdgAAAABJRU5ErkJggg==";
        private const string s_Rotated = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAHdJREFUOE+djssNwCAMQxmIFdgx+2S4Vj4YxWlQgcOT8nuG5u5C732Sd3lfLlmPMR4QhXgrTQaimUlA3EtD+CJlBuQ7aUAUMjEAv9gWCQNEPhHJUkYfZ1kEpcxDzioRzGIlr0Qwi0r+Q5rTgM+AAVcygHgt7+HtBZs/2QVWP8ahAAAAAElFTkSuQmCC";

        private static Texture2D[] s_Arrows;
        public static Texture2D[] arrows
        {
            get
            {
                if (s_Arrows == null)
                {
                    s_Arrows = new Texture2D[11];
                    s_Arrows[0] = Base64ToTexture(s_Arrow0);
                    s_Arrows[1] = Base64ToTexture(s_Arrow1);
                    s_Arrows[2] = Base64ToTexture(s_Arrow2);
                    s_Arrows[3] = Base64ToTexture(s_Arrow3);
                    s_Arrows[5] = Base64ToTexture(s_Arrow5);
                    s_Arrows[6] = Base64ToTexture(s_Arrow6);
                    s_Arrows[7] = Base64ToTexture(s_Arrow7);
                    s_Arrows[8] = Base64ToTexture(s_Arrow8);
                    s_Arrows[9] = Base64ToTexture(s_XIconString);
                    s_Arrows[10] = Base64ToTexture(s_OIconString);
                }
                return s_Arrows;
            }
        }

        private static Texture2D[] s_AutoTransforms;
        public static Texture2D[] autoTransforms
        {
            get
            {
                if (s_AutoTransforms == null)
                {
                    s_AutoTransforms = new Texture2D[3];
                    s_AutoTransforms[0] = Base64ToTexture(s_Rotated);
                    s_AutoTransforms[1] = Base64ToTexture(s_MirrorX);
                    s_AutoTransforms[2] = Base64ToTexture(s_MirrorY);
                }
                return s_AutoTransforms;
            }
        }

        private CustomRuleTile tile { get => target as CustomRuleTile; }
        private ReorderableList m_ReorderableList;

        internal const float k_DefaultElementHeight = 48f;
        internal const float k_PaddingBetweenRules = 26f;
        internal const float k_SingleLineHeight = 16f;
        internal const float k_LabelWidth = 80f;

        public void OnEnable()
        {
            if (tile.m_TilingRules == null)
                tile.m_TilingRules = new List<CustomRuleTile.TilingRule>();

            m_ReorderableList = new ReorderableList(tile.m_TilingRules, typeof(CustomRuleTile.TilingRule), true, true, true, true);
            m_ReorderableList.drawHeaderCallback = OnDrawHeader;
            m_ReorderableList.drawElementCallback = OnDrawElement;
            m_ReorderableList.elementHeightCallback = GetElementHeight;
            m_ReorderableList.onReorderCallback = ListUpdated;
            m_ReorderableList.onAddCallback = OnAddElement;
        }

        private void ListUpdated(ReorderableList list)
        {
            SaveTile();
        }

        private float GetElementHeight(int index)
        {
            if (tile.m_TilingRules != null && tile.m_TilingRules.Count > 0)
            {
                switch (tile.m_TilingRules[index].m_Output)
                {
                    case CustomRuleTile.TilingRule.OutputSprite.Random:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 3) + k_PaddingBetweenRules;
                    case CustomRuleTile.TilingRule.OutputSprite.Animation:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 2) + k_PaddingBetweenRules;
                }
            }
            return k_DefaultElementHeight + k_PaddingBetweenRules;
        }

        private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            CustomRuleTile.TilingRule rule = tile.m_TilingRules[index];

            float yPos = rect.yMin + 2f;
            float height = rect.height - k_PaddingBetweenRules;
            float matrixWidth = k_DefaultElementHeight;

            Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2f - 20f, height);
            Rect matrixRect = new Rect(rect.xMax - matrixWidth * 2f - 10f, yPos, matrixWidth, k_DefaultElementHeight);
            Rect spriteRect = new Rect(rect.xMax - matrixWidth - 5f, yPos, matrixWidth, k_DefaultElementHeight);

            EditorGUI.BeginChangeCheck();
            RuleInspectorOnGUI(inspectorRect, rule);
            RuleMatrixOnGUI(tile, matrixRect, rule);
            SpriteOnGUI(spriteRect, rule);
            if (EditorGUI.EndChangeCheck())
                SaveTile();
        }

        private void OnAddElement(ReorderableList list)
        {
            CustomRuleTile.TilingRule rule = new CustomRuleTile.TilingRule();
            rule.m_Output = CustomRuleTile.TilingRule.OutputSprite.Single;
            rule.m_Sprites[0] = tile.m_DefaultSprite;
            rule.m_GameObject = tile.m_DefaultGameObject;
            rule.m_ColliderType = tile.m_DefaultColliderType;
            tile.m_TilingRules.Add(rule);
        }

        private void SaveTile()
        {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();

            UpdateOverrideTiles();
        }

        private void UpdateOverrideTiles()
        {
            string[] overrideTileGuids = AssetDatabase.FindAssets("t:RuleOverrideTile");
            foreach (string overrideTileGuid in overrideTileGuids)
            {
                string overrideTilePath = AssetDatabase.GUIDToAssetPath(overrideTileGuid);
                RuleOverrideTile overrideTile = AssetDatabase.LoadAssetAtPath<RuleOverrideTile>(overrideTilePath);
                if (overrideTile.m_Tile == target)
                    overrideTile.Override();
            }
        }

        private void OnDrawHeader(Rect rect)
        {
            GUI.Label(rect, "Tiling Rules");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            tile.m_DefaultSprite = EditorGUILayout.ObjectField("Default Sprite", tile.m_DefaultSprite, typeof(Sprite), false) as Sprite;
            // tile.m_CheckAllTilemaps = EditorGUILayout.Toggle("Check All Tile Maps", tile.m_CheckAllTilemaps);
            tile.m_DefaultGameObject = EditorGUILayout.ObjectField("Default Game Object", tile.m_DefaultGameObject, typeof(GameObject), false) as GameObject;
            tile.m_DefaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default Collider", tile.m_DefaultColliderType);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(tile);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            var baseFields = typeof(RuleTile).GetFields().Select(field => field.Name);
            var fields = target.GetType().GetFields().Select(field => field.Name).Where(field => !baseFields.Contains(field));
            foreach (var field in fields)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(field), true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (m_ReorderableList != null && tile.m_TilingRules != null)
                m_ReorderableList.DoLayoutList();
        }

        internal virtual void RuleOnGUI(Rect rect, int arrowIndex, int neighbor)
        {
            // TODO this is where we add custom rules to the GUI
            switch (neighbor)
            {
                case CustomRuleTile.TilingRule.Neighbor.This:
                    GUI.DrawTexture(rect, arrows[arrowIndex]);
                    break;
                case CustomRuleTile.TilingRule.Neighbor.Empty:
                    GUI.DrawTexture(rect, arrows[10]);
                    break;
                case CustomRuleTile.TilingRule.Neighbor.NotThis:
                    GUI.DrawTexture(rect, arrows[9]);
                    break;
                default:
                    var style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 10;
                    GUI.Label(rect, neighbor.ToString(), style);
                    break;
            }
            var allConsts = tile.m_NeighborType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
            foreach (var c in allConsts)
            {
                if ((int)c.GetValue(null) == neighbor)
                {
                    GUI.Label(rect, new GUIContent("", c.Name));
                    break;
                }
            }
        }

        internal virtual void RuleTransformOnGUI(Rect rect, CustomRuleTile.TilingRule.Transform ruleTransform)
        {
            switch (ruleTransform)
            {
                case CustomRuleTile.TilingRule.Transform.Rotated:
                    GUI.DrawTexture(rect, autoTransforms[0]);
                    break;
                case CustomRuleTile.TilingRule.Transform.MirrorX:
                    GUI.DrawTexture(rect, autoTransforms[1]);
                    break;
                case CustomRuleTile.TilingRule.Transform.MirrorY:
                    GUI.DrawTexture(rect, autoTransforms[2]);
                    break;
            }
        }

        internal void RuleNeighborUpdate(Rect rect, CustomRuleTile.TilingRule tilingRule, int index)
        {
            if (Event.current.type == EventType.MouseDown && ContainsMousePosition(rect))
            {
                var allConsts = tile.m_NeighborType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var neighbors = allConsts.Select(c => (int)c.GetValue(null)).ToList();
                neighbors.Sort();

                int oldIndex = neighbors.IndexOf(tilingRule.m_Neighbors[index]);
                int newIndex = (int)Mathf.Repeat(oldIndex + GetMouseChange(), neighbors.Count);
                tilingRule.m_Neighbors[index] = neighbors[newIndex];
                GUI.changed = true;
                Event.current.Use();
            }
        }

        internal void RuleTransformUpdate(Rect rect, CustomRuleTile.TilingRule tilingRule)
        {
            if (Event.current.type == EventType.MouseDown && ContainsMousePosition(rect))
            {
                tilingRule.m_RuleTransform = (CustomRuleTile.TilingRule.Transform)(int)Mathf.Repeat((int)tilingRule.m_RuleTransform + GetMouseChange(), 4);
                GUI.changed = true;
                Event.current.Use();
            }
        }

        internal virtual bool ContainsMousePosition(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        private static int GetMouseChange()
        {
            return Event.current.button == 1 ? -1 : 1;
        }

        internal virtual void RuleMatrixOnGUI(CustomRuleTile tile, Rect rect, CustomRuleTile.TilingRule tilingRule)
        {
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
            int index = 0;
            float w = rect.width / 3f;
            float h = rect.height / 3f;

            for (int y = 0; y <= 3; y++)
            {
                float top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }
            for (int x = 0; x <= 3; x++)
            {
                float left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }
            Handles.color = Color.white;

            for (int y = 0; y <= 2; y++)
            {
                for (int x = 0; x <= 2; x++)
                {
                    Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
                    if (x != 1 || y != 1)
                    {
                        RuleOnGUI(r, y * 3 + x, tilingRule.m_Neighbors[index]);
                        RuleNeighborUpdate(r, tilingRule, index);

                        index++;
                    }
                    else
                    {
                        RuleTransformOnGUI(r, tilingRule.m_RuleTransform);
                        RuleTransformUpdate(r, tilingRule);
                    }
                }
            }
        }

        internal static void SpriteOnGUI(Rect rect, CustomRuleTile.TilingRule tilingRule)
        {
            tilingRule.m_Sprites[0] = EditorGUI.ObjectField(new Rect(rect.xMax - rect.height, rect.yMin, rect.height, rect.height), tilingRule.m_Sprites[0], typeof(Sprite), false) as Sprite;
        }

        internal static void RuleInspectorOnGUI(Rect rect, CustomRuleTile.TilingRule tilingRule)
        {
            float y = rect.yMin;
            EditorGUI.BeginChangeCheck();
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Rule");
            tilingRule.m_RuleTransform = (CustomRuleTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RuleTransform);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Game Object");
            tilingRule.m_GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), "", tilingRule.m_GameObject, typeof(GameObject), false);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Collider");
            tilingRule.m_ColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_ColliderType);
            y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Output");
            tilingRule.m_Output = (CustomRuleTile.TilingRule.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Output);
            y += k_SingleLineHeight;

            if (tilingRule.m_Output == CustomRuleTile.TilingRule.OutputSprite.Animation)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Speed");
                tilingRule.m_MinAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MinAnimationSpeed);
                tilingRule.m_MaxAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MaxAnimationSpeed);
                y += k_SingleLineHeight;
            }
            if (tilingRule.m_Output == CustomRuleTile.TilingRule.OutputSprite.Random)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Noise");
                tilingRule.m_PerlinScale = EditorGUI.Slider(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_PerlinScale, 0.001f, 0.999f);
                y += k_SingleLineHeight;

                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Shuffle");
                tilingRule.m_RandomTransform = (CustomRuleTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RandomTransform);
                y += k_SingleLineHeight;
            }

            if (tilingRule.m_Output != CustomRuleTile.TilingRule.OutputSprite.Single)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Size");
                EditorGUI.BeginChangeCheck();
                int newLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites.Length);
                if (EditorGUI.EndChangeCheck())
                    Array.Resize(ref tilingRule.m_Sprites, Math.Max(newLength, 1));
                y += k_SingleLineHeight;

                for (int i = 0; i < tilingRule.m_Sprites.Length; i++)
                {
                    tilingRule.m_Sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites[i], typeof(Sprite), false) as Sprite;
                    y += k_SingleLineHeight;
                }
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (tile.m_DefaultSprite != null)
            {
                Type t = GetType("UnityEditor.SpriteUtility");
                if (t != null)
                {
                    MethodInfo method = t.GetMethod("RenderStaticPreview", new Type[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
                    if (method != null)
                    {
                        object ret = method.Invoke("RenderStaticPreview", new object[] { tile.m_DefaultSprite, Color.white, width, height });
                        if (ret is Texture2D)
                            return ret as Texture2D;
                    }
                }
            }
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private static Type GetType(string TypeName)
        {
            var type = Type.GetType(TypeName);
            if (type != null)
                return type;

            if (TypeName.Contains("."))
            {
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        private static Texture2D Base64ToTexture(string base64)
        {
            Texture2D t = new Texture2D(1, 1);
            t.hideFlags = HideFlags.HideAndDontSave;
            t.LoadImage(System.Convert.FromBase64String(base64));
            return t;
        }

        [Serializable]
        class RuleTileRuleWrapper
        {
            [SerializeField]
            public List<RuleTile.TilingRule> rules = new List<RuleTile.TilingRule>();
        }

        [MenuItem("CONTEXT/RuleTile/Copy All Rules")]
        private static void CopyAllRules(MenuCommand item)
        {
            RuleTile tile = item.context as RuleTile;
            if (tile == null)
                return;

            RuleTileRuleWrapper rulesWrapper = new RuleTileRuleWrapper();
            rulesWrapper.rules = tile.m_TilingRules;
            var rulesJson = EditorJsonUtility.ToJson(rulesWrapper);
            EditorGUIUtility.systemCopyBuffer = rulesJson;
        }

        [MenuItem("CONTEXT/RuleTile/Paste Rules")]
        private static void PasteRules(MenuCommand item)
        {
            RuleTile tile = item.context as RuleTile;
            if (tile == null)
                return;

            try
            {
                RuleTileRuleWrapper rulesWrapper = new RuleTileRuleWrapper();
                EditorJsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, rulesWrapper);
                tile.m_TilingRules.AddRange(rulesWrapper.rules);
            }
            catch (Exception)
            {
                Debug.LogError("Unable to paste rules from system copy buffer");
            }
        }
    }
}
