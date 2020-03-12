﻿using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SeventhHeaven.Classes.Themes
{
    public class DarkThemeWithBackground : ITheme
    {
        public string Name { get => "Dark Mode w/ Background"; }

        public string PrimaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string SecondaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("MedDarkBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlPressed
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlPressed") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlMouseOver
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlMouseOver") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlDisabledBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlDisabledForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string BackgroundImageName { get => "7H Icon"; }
        public string BackgroundImageBase64 { get => "iVBORw0KGgoAAAANSUhEUgAAA+gAAAPoCAYAAABNo9TkAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAc" +
                                                     "dvqGQAAP+lSURBVHhe7P1Zjy1LlueHufveO+JMd8ysvDlWZlVl1jx0VVePpEg1KYDkg/RC6kWAAAH6AtInkL6EIEB6EvRAEHyQBIHQQELsp" +
                                                     "kj13KweqrvGnCorh5t5xzNGxN7u+v3+ZrbDI86OOHHulPfm9XWOhZktW7bM3Le72fr7Mjfvu4UWWmihhT419K2u6/93xP8rwhnhazJvSP+E8" +
                                                     "IvUf4f4LuE2YUd4mdDoDcKPkPl14u8R7hA2BOkFwuuEE8q/SIwcvHvdtrvTr0i/073YvdT1/a1u6n7S3eo+B68j/eNu19vei4R3uzH6evj34" +
                                                     "d8ifY/wkLyyj47Hfr2Z0u6TnvRx1w3wLT3rp/7OuuvGqeQfTFN/TFp9sLvHpE8u8bbdpn889bTX9SP5FWV3KVmh9RGyW3hnVf+mO+ZcrJRF9" +
                                                     "9h7buQ/QY4IWnHOBo7xnE66qT9F52WyrV1qSwPhg5yu1T5F4/oKvXf7YbJV6QnhXeRHzoYk/7jv07nbxEfoUOZ+d4rUNjqVseyY9LvwLRn6Pi" +
                                                     "3fgn+rO5tGcqfkrcvP3t0bhsnrgOsj/DVyhOjidHZPOCHmPcNbBPodPyj57a7vdo97zv4w+ZvZ/lvE5u9R+wHpDTz78hPSQ9dPn0XLj0mv6O+" +
                                                     "LnGmv1XvdKfUfp/wBV+dn4HnEXu9cr9MjYvvl9f4dwquENwlcg9NrxI08HoP9/j7hq4Q/QOZvEd+UfkhQx39F+J8RvP6hpy+UhRZaaKGFfubog" +
                                                     "5zxF1pooYUW+pjR/5Nx/l3ibxAEET9HEMRcR/cJ4FoBeQ8gD9gxFnAeEQJmCcoIyAESvYD4IUHgbSxw9gEA4Kj/DPDoDaDQJtAWeAbOuA/MeQ1" +
                                                     "walsjUEaw8zCQdOoF8m/CtZ3bcB52oDPoaC1EnPozyja3wSpw1bVdA6pF1ADhx+DGu/An0wBfQdIt4vQZOHcKLvUY7Ltw8AE8WxU4vkwPp2nVP" +
                                                     "yB/Rv2BPr9CTQHh/WnXCzIncgC/PGx4UsA1wF2+IL7PQ4uB9h5TJl/AWVFVjsG8bYvJORbBfUG6kAJzwGyfDJIy5yWVPLjryEpNeaW5nvl14BE" +
                                                     "U+F3Isg1HkgwVBM1VVaLWL8H5LcC8ILg8UChnwq692A+j14APVSZKBcn++gLnt2jtjLDmBxQ43wWMP6QVzlvOwZHgnl/a68y2vWjudiO/Th/eE" +
                "2Lq2M9pi/yWDgnWBfUnCPCb0Rn6cdZ3ZwT/3RqH6RGX0i3KbtHDt9FDWu3R+WK3og992nuNPguub3c7QPtDyv213qGv9ueUeuPkNaAOY8+d95nX8bcIv0C/1Ol94bkQ3Psg6Z8Rfo/wDwm/j4z3" +
                "1mcJz6K/IHyF8HcJv0r431D3/0C80EILLbTQzx49Nd8vtNBCCy30ySSM917v7F8hNCBwExIkAKp7gbUedb2M6hFQNBKsAL57PY1fJuh9fIUg+ACk9wITvX4vAtlOgCVnQKU10EWZHwK3Pg9fYP2" +
                "g2/UCkvvAHTH1Dt4jALhygsQn5IcVwHU9xmN9fBsAC9bagWg3t0wJ9Yone0PQ461H/i5pwdODaei3k17WidYHfaL9Q2QEhy8icQIgFzCO5H8OeIe+/v5EW9Q5Ii/A1jOvl91jMw+oBrSPwD8SJd" +
                "8dUQ5gD7gf4NJrAD1lYFqRoZBUUr7A9CS6FckTdA0U6FdXtun1eAf+NQ/0nNTmSoALNEfYh0hxT9aMPG7bmpMg2jNihdYPu5Y0Gc4dJx2EXCsalV+hkMffj/2kHvuIrnTUBxkDgFmgah1r+DvcR" +
                "ro8mLGg52rxUUnvtZF+6J3Xg/8O+ScEwfRL5IXJAvgNXXkxevXM29EBsO6vzu9POEPnS5SjO15v9JHsu1M6uNbrjr4zFHFZBNBvnwzl4QH84hEfAOJDgPo9+N4fW86B18sPSN/NWgu9//fV5DVC" +
                "X9/OAzBlXybvtf1tgqtA1Ok944oRZPY/on1X5pcJ/5jgvcd5mXwYdhP6V4TfJPx9whcIPhQgWmihhRZa6BNOdbpdaKGFFlrok0iC8v8z8f+eIADSc/0sErT8OfV+ifgPCb9DECzN6wocvoTMHxE" +
                "LGBpYFQABIgKoBSuvAWFW3Z3+h8CszxPrfT5C+oj4PkBUkL8NmBG4TB2gKw8RBMmngrE7U79mJgKa9f0dl0Fn1bLYqgNY642OR/0J6fvTNLxEXID4ptsCwfRQHtPzd5A7o+rPAbVOaMcl5rdIu8" +
                "T5HQE4MO4OUx5AP55wIbPHI8g+pdy8eFcY6sMAkc4kfPORwCrAFRmh3SpL5E9WUy8KQ55jtaxQXfLeu9a6py3Pm/0N6EWpk66rAAD2ZkOWzZGV8gI8yTLa85Q0mF/pWei8kVfFufbmJV9xYOqUv" +
                "B4ClivZZ8tK8FD6LHfPMcBUo3p2lT8A3jkH0WdL/Nbxag9UWHOwq7Hn1y3rDCz39PbU4dxRx0cG9meY2rJwOAHo5m3HBwD36K/L7G37HmV6yN+mJc/JywHv2+4Rv5yA+i7xG+QHGrgnTiftMvUt" +
                "Ol4dhslr8SEH4hJ8QvcYpTvSOwpWp8PkUvyzBzn66RVasK7nwLUUPmx4AY2PCD648IFK8ZxP3lPcAz+eOC/w3phepbceg+fGZfGmvce+SRCw/xkBUD55DUv2y3NsG95/XvfQ9PUSX0ueJ+v9c4K" +
                "6F7C+0EILLfTJJWeghRZaaKGFbkD/22eMmX+TINj9fHJPk8thfXf1T5K7nvSqabi7rFXP9L+ued/h1iuHQZ73vA+RINpl3MYvIieoqel4wtG9Pw69nlIDRy5Z/wLlAgUtfPWUso3gCJB7D2C76t" +
                "4EBn2G0reANoJwFfoutj5pQHreDY+fmoJx0PsMKN4AWMGVgOfuNkjKpeABscR6VG+TFjS/Ax7VQ00a8OxbwV1/CyjnO96A9P4I4HyXFvVqd0CkF2j1MWmXm3uM9kMQHJCLnlN6IBwEZwuL7RLHI" +
                "tye+hGg7QMCgZZw8SSoKw8B0mfPwYk+fP4JidWpP/YJoF49lovFRduCd/NnExiPWO+6nlkfWHA+ERFO0jca8vx6viUfHihvH0dkPIeNTAXMR38B6dY13JRKq+opseTR26YEH/WWFL+/JZY1MK/H" +
                "2veyvZLaQwLOt/0JKEd62nAEI5oExQJN+Cnb8OvaKv0NX486gH7yNNvWGZK2v9pynkZ5u/x+wfOjHnjXNnBdcQpOI5uHJdMRestDguLx9ho1/WK/GvV8n/Dvbs+PS/otNPre+wvoepOrweXz92j" +
                "gYTdmaf6L8F16LxDXe28aHROgvTujX7cB7f7ajx77rIZ+7gZ+w7JM3lUngneP2PbvwIse+vSA9CZe9u10H0jv2hLCZP+8TiV/U5ev61n/S2Lvd8+x94HkfVQfkk2OHYJv6/gQ51nkg4A/o963iH" +
                "+F8P8gtIcB75MmxsKFFlpooYU+JHLuWmihhRZa6Aqqhui1Y6VLyvVyaQQf8mu6LFyQLfjV23YVCewFu2VztK77LoGGewH97xIEh4c85IITEYKAJQCXgJ7epbUufZU/N8z1gAuOJA/MoF6BsMBBs" +
                "HwPjnDrtHs53vIHtP4ykoDiApzhSYInAaSg8hHhGDC+XaMdJHMbpeKsNQgGUEonCwB/exx7AYbpB5O+U/kr4Ms6XvFXhDUBsX3/Emk94MISz83D4u0GfAO8kbHvtO2SdNL0xj8gmGwMN63TF13f" +
                "dsnenqzHQfSsh1/IKfi25Jia+kP1qCsYvSQB1QH+gqIAcTTbL49Hnu26JB1whroG/n1QUR6iCE+NBcCUxdNuV7K+GlKH5y+ZkG3mEPa8BsbNN4Ccw7kBKWc9Y/UY22YjeZTDLq1U+ZxGOcpmGQH" +
                "kdUQ+ZVUy/PKYpMkLsqN1ErD6zrhHJP+02+VA441GzCuHazfL249A4v5mrm7Qg+4CBYG0vXnMyRPU+747uqYTuOZXnnykkXcpPnpMUYd6gnnPFRA/S+9dMWCfTmjzZYC73uZTflm98U9o1XZfGn" +
                "rSJxxnP931qRH6fKjmcnqPQ9Dug4iznQ97+mnLge8ecyz04uQUGdr0YUwB7cVT7zvuT9QH/13Sx+S2pHfdm9TacT89BOiX35X7G975igaBvPctHZlcdeL9om5/E8v9PXyAxn2Rd+OfRXrl/yXBs" +
                "eT/Qp2fJ3ZcMnwAtID2hRZaaKEPiJwLFlpooYUWukTXecs1il32LSh3mfh8F3NJwCw4bsavBjdG9lOkUW1dg6Bc4/u/I6A7S8v/HYLG+GXQr2FuEKjalob766Tb7uAC2QJOiiFPXwKI1W9b9kXD" +
                "HyCfOoKVF4ASwOv+fvdC/1lqvsXhC9YfghmU9d1fdbhVloBYwOkmaXdvx1vdDbcBwf2Y5ekCVN8NvwfGcbM1Pcbq2HHm3Pn8BXonMAfkwN/EE36UBcR9TztZir4FiNtPgfTJNOp4p1e0i05B8wj" +
                "yHUB9wqLtAK5B4CjoDOC1GocN/RAcF8Ad7zftDfbL08KxCAdXxduN3IZ8BdIB4zSjE9fDice7gMyinxCdBcJNAd1A6n4UGlLiHyPPg0V659WTXBGtOuhj/hY9JVW8xOq8TKWdc+K3nWefIvrblK" +
                "Ztr4Xahz2ZLw8EWkl5YhF+ZXnSTcMPx1/Af+qnbuR3HLyHJYD2oYzkz+G54Ly64j+APXx+tdJmmig64Xne6WXkPOFu2ef77ObVLaj3VXgrCPi5XgDcAvV+coXFGUWngObjrWXIonrHH46NUs+WX" +
                "veyNJ1MALQA3dcyfGjgZndvA4tfyGOVsXuXK/dV2nwXzV5pL/ZbrlUfRHXTZ4YhIF/+HS7E09LpqXvo2eC4H9Pi5E749sv37l2G731XPPUA9uSPaedNePcA7D4maIBdcP5Z2mkrXKqXPeOK3nbP" +
                "lUE5ZQx2AdA9OR5I3jPt95+T14Fjkg8LHSv+PnX+BrFjle/JS39QY8mHFO+VGEfz+y600EILLXRzOjR2L7TQQgt9Kuk6UC65vFRgLijX+zQnDWQNWb1UGroC50MkyPLzTV8iaIi72ZpAQXBIFP4" +
                "heqDVT6zxrYGvniN4GuWC7gbo7Idtt43bBPINuGvQ0wj/Nxjpx+i6CyAXPNzu8z445YJuQbFGvB77s658ymwHmnYDt9tugb3SKw74ARj7STI/I+a73Rr790FatuPnxO4Dqe4E424i8xI9fhewfU" +
                "RvBduAYwG474QHQMcb2kAuvBEg7nnZ0BY14u0eNuWdb4G/KE3P/Y461pPvUZ6M3WDfB6DzWXC8BaIn5YSGvgPuOdPzLmARlHsMfb9Cg3kQKbyV6JP+iKkBzvDMUhVNEIoF9Mmnbf/UPH9tiqhWK" +
                "GQf+DPjBcx7voNaTbdwiM7RTqk3J8vOy1t6XuMwNQnjdZBsy5W/hvJMoTKhBprpc3ieVfRHpsj3005N5LhMFEPE/30AfJ5+8Gu7NoKapAXjRVfRgxxno6VN6A33yqRtASqgf5y4JiJzxC8pQOcM" +
                "xvOujH28NQ3+xNxjAPBT12ogJ2Nrfa4cartc3fZdjq/n3WXuylsmsH6Hu0DQfkLdHa3f43De5gq8R8NeYfeJX4IXsExdd6R3l3nPwXhKjPZH3Fjr7dDd3Q7ZuO5luN577mDvMvtXuhXtlAdHr9P" +
                "Gy9wx73SPqPnQNrwms/GcoNvT6ioXjy/HTmxoJJj3/fXHlPnVBuv5sK2NEY4h1LtAjlnK+Dm43yKu77+HfK9dag8cJce590sLeF9ooYUWOkyXx+iFFlpooU8dPQuYu4Tdd8sF5YK9OWlc/xvCda" +
                "BccudyAb5g2s3V9F4LngXFfprsELnkXKNfg1qALlDWOLeOHunWF3eOpqxvn3zSWFeGg8pS9DX/hu6of0TpHUDnQyTuYRvr+RbU2g+XjO/ATHlbmrgHCAt+ju6SFgwLyME/tnNCDF6IV1xP+C3Sa" +
                "noEPHJJ+gMw2REQyqXgYlr6omdcH711szzd+jvKAXkEYdeqH9eA7CFLnPWKD0ccoH1y4+1jdLv8XdTkkvJttvKKV7w/Q0/e/q7l1e6P517gLdjmqBQXayMjv+BkGg8oLvUI5V/qww2vFJSHAOWc" +
                "B60G+LshGAqTtlUfFgiikrbuvk4hAKl9rGCpoFv12z9OlcnkJUstv0y1P5WKb7hmhKiz8lJ7rsNfV6oyOcAKhKGcQFUkI9e0dXLSLOefefmcutHjqDrLUgSIfAC0/7169GBbYmP8b12A5SFbaE8" +
                "QqeBdz3gepIDf3VzOU9YkUr0voLvogE8b9ksZgbnL2bmK4kFfD6iijg+u7vT9bksfuY66u/2wE4xPMLi2AuZ3IPx47a2dBRzjtAaYc30Re9x5aACwF8BPfkYu4P0el/opMaoA7afxTL8wjKPjgR" +
                "vT+Tk4ve32y8++nRg/6rsdbaxPXbnRT35xAHnOFWcLWWM34ROwu+z+Ia0P3Vu0fdq90Z1m0znu23jbHYO8z73OjCmrp6esmrFtY8uRn/ymOucq5EO8+rAvY4rkOOL58Id6A3k/63aZmlfdce+PS" +
                "zL7ZNj++6EFtC+00EILnY/RCy200EKfOroKmGusagD/bYLAfE4aroJmPUhu6nTV8s/mtdKD5cZPLhkFPPcatHrgbWMO3LRKDRjSgTmC8p+Q1oA2ryFuHY1mw/cp02Mtz7aUfwTPNjXY191LQIUN" +
                "9e7ooabtVeeu6upRo55nN1J7gfg+MYgaZSCVo4D0/pgz42ZvfspM/Y/GcXADrCdAlVsgIrcL2yHqTun3wVIvqz/ecZf0jvnWOFDHZd56uYEuBbCv1uofuhNQ2NHKb5oDt4hpp/cb5YI8REjTJXT" +
                "4pWy64qJjvd4Bv4DgOHR9BTmoDr5tcT6Bc8DGAS2U+U6wm4UHfxorF0yY391gJ6kfZOz/gkFl8/OUhc6W+QsE9QVsg6uKzvTBSoIqMaTpVO8F4jWZ3xMdHkN+O6L02WIfUDS0KcmzTAnZrSiqZm" +
                "S+8Ypc2k+6NgwVHZWSnOszeLjEM7HwyZejJqSsydDKeb4cTM6YecmO157nJFnOccMzKYiv76YnC3liFC5Jvejx4pcq/jQRErdLFqs5mw4oo07rC+n1MvPjR5I//nTEgvcosz9hC7TXVNjmbu6mN" +
                "WBaAJ/9AQH/O9hcx+F1p3rkwfGidi68lQ13Q45hg6wg/Ha/cpO6eO4fo5O66N9OT/rd9ALhbXrx4rCjzq57jIxyeth9t/2UI9rS5/UT7pszzg2xXnsfHriMn7a4V901vnjZ/RSdO9X/uHs0vcid" +
                "9k73k7yf7qcRXRKvnIDbpfCeLI/dMUFqIN4yx61X7DPxZXKMaHU8/QZ5f4T8X5X5DJqDdneqdzzyIaDxe6EFtC+00EKfNnLcXWihhRb6VBCGnnTluCco91vE/xZBj/ecNMRdVirQvgqUS75L/lW" +
                "CjbgZm0vYNYKvWrouCADw5z1mDGgBa74dLgAQgEt6v8hXb3gxlvWQuyxcebemetwd9Xe7u8iu48V+hHmOnAA5AFswfkxapHN6NPabFekNIBbM4u7qwg7xlV5n9T8eh8H2HwE1BMx+fRpADnhY9Y" +
                "+RcZn6E6AqAD/fGY93nLASfoOJdkId2liB2E5XfcC4+gW3viv+ZOzdZR29gmmBL6BfeAUPYEGDBfdhmcMfYVMIvOB4Bs/VTiwfJ7q4yxjxgjVLWh3WR40Z2KpOuZqBcQXJ0Z/WPufGMmX8W9NFb" +
                "8GR8R7rEJa9Lytd3ZP1Uzbj5zhapqKNyJRkSShTk+cFUJVvhbYZXuO3uNF53dqDKpwoCs6rtGJJfhUjba4cONmwTVT5HGCS+aWSlFfTEU0Mz23Yk86/JOCXdDztVCItkE71BEA0cLQAdi4fn2EU" +
                "NekPcuUHSWx9vfn5KSB1+9+UHIG7/fSJjgzbSj5t2a7toY8y1XGNCuCzZH5dOqWC6ZjwZNdPt8ZsaDdtdy4syZMC9ynIzvQuca+rM0ZXJpQLNyLTKXfjXdiPuBtfFLjDPyborPfBRb7JTh2fDOj" +
                "BX2+H6eG27+6dDpNfeXfXeJfeq7+A9uJhfwd9r3Zn0+tw7vhSByNPXUETD7vg3fHEwBgytXFFcuNKgbNjjkvilT9EjjvqdMNLZTmu/BBujOn9+Cxy/Jy/3+4YKmhX3/ugZWO6hRZa6GeSyrS50E" +
                "ILLfQzTM8C5i5d/zsEd1G/bKBqSLp0UyNXz/kh0njVUBXU+ykkDM9esG+DhwxeDVPKeg1jgbdLS5WTP19migG835ld41igLXjV0L7TvdC/i5n8Iib/Q+Lb9MJ3xKmf49RP6DvWvr0qwt4Bwo+Op" +
                "g580W3MAxkAQHmX28+bnca3CeZFj0vJjwH9j8ANd8kLyAXTAJbEAObohq2zOvB4R3JNocczDVMPwKHv2UANdIUIOvUXE+kw9zjMeIzY+gVC8Vfcg7r8t9jTJMqpYFocIy8YrMjpYlSYP+pQykas" +
                "SyXRV+QL2wq0A09AXvQT+FfS5Ziabs/1ub4UlIcHhe/f1DFV6pdzIQw0YXdk1nKo9sYCmTXUdqTEtV+VfPUgbc6ZT5G9qHrSoasImQjlgC5q9KBt2xOa7gZ6wkPOamHWGuVwjIM9/eE83og3WVt" +
                "ImfnoVKTkkzYhn/gC38shZx3cLLP0FZ4QG3HOrelcBBSkvqsliF2Bnj54+i22dpUXB6vOHnOJjqOedPWWBfU0hpyx8mhJGdcK8egn1wDgIyDbd9X1vgtM81bGtBO0I3u6BVRHO6B751J7D8UOuW" +
                "og31ePfi984x13xhpAbceMn1B8e8in3wDuvuYxGHMjuf8BQk+IAe27s757AX0PONKjemuckPY2cYf4I0aCd7l7X6b1d7s327vrAeYud5fkvQlP77vjiQBa77vjnD+gv0vbu+Iq8p15A+dJHdNvy" +
                "IQOjaOHyHbaVy3+Xo31vhveBy2gfaGFFvrEk+PqQgsttNDPJGGoXTnGCaB9t/xvES4v89SI1dvjJ4k0XA+RBq7edpewa2j+EW35bqcbwAmm56QFbXAJurJ6ypVpQV16x30AILg3rZdKz5R98x3r" +
                "LSa2y9WfEB8Tn2FCW1evr+90u8haQIw9350MY5asr0TCvrwNn/KAcZesn1Kczz8BFe7RyuNpNdzGjH8IhgnQn6ZB3QJyAIdwIsvUATCke/T2/QmI7Bj99HVYgRGCeSgEsMQTbrwlBsQE1Lo9F+c" +
                "ACOJbxXlnW6+9GFqXIFkYsgukFiuK+URdtFpiwI4d8FAjFQCOnGmPUeimGuVtK3qQz3LzAPmQvUWuyNpneUlb51LgT5BaVKnHpDCrlhtqPSlKPT4z4LSUm/HcVaGCdouuwCt/Q9OVkvaPrXj45u" +
                "c0ay+U3tyAmk5iohz4vmr6BMkryoGlJMzn54WU4VgEyWFYpqd7KKI5KzVWh57r5OH6y1hMRRGssbLFE+3POKuvrnqK4KWeL0gUL3rqEoq89aJQnnXStVo/csnnEiSLcMr4Y6wu9SrHRaTjXFVcS" +
                "KbE7+04ZLY+ehmSyKEXefXo5JdxxLk4pf4xYcsJOHIDBZoCjftQwF3kAe7Wz2U7grWzKd0Jeu4QHnJnH6H3BMB+m/gM6OtDAt/aKH3hL3WHM8A4BbvHg595m85I++11f6HysIL2iWmb8ewRukfA" +
                "9NuA9LF7wEjxQo6lrODx+hRg+zqNJ8NxyFYcAx0LkZmaB/0Q+bDCeu6tIeBGn30NSBesO779MkEybbgJzTen86sWtvMel8kvoH2hhRb6RJFj6EILLbTQzwxd5y0X7P56Dd+QcYl8X9L3yl32qVf" +
                "pEAnK9Sxp2Aq4NWD1TGncXqbqce/re6F5f1pDtslqcArI1aGx3LzuGLAB6VuA+FF3h76s+Hc73vDiIZ8EzxrMoAU0ulx9hYV+BE7Q7vd9cnHGOObdbj/ztNY7DjTwPe8VprNebcAJvEEvfZa/n+" +
                "ZzZtksbtDI11/tO9xHIIoTcJied6BGDyDvdyAFJRAtaIAsEMIYlB2EkwBG8TVrMQ+xKCNpqmaBOv/ASGSsKLpQ1r5bLhqyqOCkgmjEW6nLH/P1lEUHZyIuevSoA55dlG/b8j1kehC+alWOxqIvx" +
                "10KokPyGUCOQ2UFSFvDfre2rV/qBKqS45egTsr5U8ot9o+glZzpyvenIkXav8qYkVrCawZKHQ9VOc9Dlb+WSjetUmuFV4J6iWWVPy2qLZHNiYfpT1bl4KW4naJaJ+XKm81JLsBVZjprvf1pCbhV" +
                "ToBOP5QDMKe3ZlWS8qpj5BJXTfjIlXcekhaz5oIqbcZpbXsR9nSnfs0n0EoB4+ls7TdIll+PdHvY4EIL8rl44m5XnHbtkx53fkobTH9zEU9bsqtuHI3X3CmCXm6dkXt52ngGaHXwhkXXSn0cMdd" +
                "cNp3Te7/icnMJu++ob/sdfIA63bvVneTzctypAnlGAnev9xz3evbT05Ozfjo+4ZYZuU22w+RyeD3sxOrvHta0y+uP0fcOUPqFbjc9AIILmG/Tx7YzvDeTeQG5Y5ZjnZ51Yx8cCtg5N3vv92XyHX" +
                "d1WAdwHeDfyIeYBslXgSTHPcOzaL5Mvr3b/l2C7b1XYr7wN19ooYUW+liRY+xCCy200CeebuIt/11C2/xoTv+MoJfGd70PUfMACcQ1Ev8Fbek597vEl5dyauC60ZsPA5pHXEBvXmNX0rAElMfjr" +
                "hFsxwXwgvJb3UsAZgA0YE9fuQhD77Xe9GxvRVqgPq7H/tgd2Y7GbqUbULwAKD0TlCN7BjQo746v+2NM/wfTarhDK+6i7nur6Bhc4i5+GUQO/Uq0Tv/7fgNCcDdyN3TbYvHHeQc8QLjf0S/AEjgj" +
                "4mIMcQ3/CxgX8VBCOTyxerCa74t7mKIUygvwFWmqEr2x960bXcjzP3n5SYOCIq8+xLX/Sx03t7bMPto2/4PJfP5g1nRkw0enJ0W+CCt1S3n6oh7/Uafk1ROOMvbdWGXoMePrBqJyzXyzKbcsdYu" +
                "8qaaj5CIj5W/4kS/xXrASshc4ezkoDVaqrEJmZoychBlRVLUmlcReXL5sD31G6ihCObxarcRNlvIioqz5dL/mPauNr2BhyYuMnHi1zRewrBvbPHJlIUataz0veG4EstZED+mkAqzpTsmnTS7wXK" +
                "o139oJ/OafoNz2uK0Utj4Xhpd4OR7ws22HZy3l06p60x9iO2UfbcMLKFd1HiTsSA7dbhRku8mcHvZ8Cm43jGu0bBE4HqexfJauvhCS+oMA3C5lqfsj7s4X0fmAkeUud+hjRDbcoT7gu02577/n9" +
                "RFv5zPuceIzhO51K2S4ifh3m/Qj0n6H3Q3nNowETxgdHnbvAL7HbDKnd91xTECubh8w+PqOY5A8xzHHNsomN8B03GorjV6vcSMfdgr8fRhJHNB/mRyv2iomv3Shfh+gyn8W2V/bN/g1Denv1vg9" +
                "0uJtX2ihhX7qxNyy0EILLfTJpOtAucBZT/nXCb8mY0Yadb476bJJvTyHqC1X/wJBi9zlm4T+F4gve3s0DulIPommIWpa47LJNQNSEG65QF9kiLFLnTVyt/rHmNjrtJpvg0eHXvLbhC2Y4/Ew9iu" +
                "Q8fER1v1tocCYd8P9xFneGSc8xvx3V/WHgPF7gHKMZvLr/gT8sAFZaPK7YNZ+uOQaEATqAHjrLtcrTvDd8h14Yg3uFHKM3Rq0HeShZ1wY0jzkQ3ZIF0UIv9FJSQA2/wStYpjiMq9gnKBcZPNPwO" +
                "xaX7pApgLyPuA+edrwAQAVwqe81A1+UoOqC8++2kUFaTf9I0nWI4suhSNvedFj940NyHuKklYHcXSqo8i3Mtst7ZW2Ks8oMqQa3zNm1kTNl3LTeYgB5Q+0158/jXuOCi/QebF0MXeR6EJNQSZLI" +
                "0VlKRKnSgGhSYoVkatsYlJmOMEWpawUyk5vI1HEPDk5tYokX4pCguXolDgZ6tEzrYhXC/r38jV4SalP9JqTpgs7+fwURSeRyWzcz9kGVFcdyvM38LfWCbAGv+649CJHndIuf/inLHwvogB7gudG" +
                "IG4nkeA8EJV66vZScnOGYH/1lxhl6aPHEB20t6Vs3e/G8vm2sXrekRn70b0Ud2fGVPUdENpzWbz6re/t5snrqO+rLU+63XSXq/thdzrdQ9cDun136EYfNgraR5+RnRDv+unkSd/dGofpjF6P3Ab" +
                "3CIJ1/w0Ad7/20HfvTk+6+/THEWvMJ9kkxz83wIRrn/YPG90E07GW8TTfXHe/DINAXg0Cbz3tnlDHvn9BYJyMDsOzqC2Tl9q77X5K8vKD0UPkQ1c97nrY9bRL8q56belZtHjbF1pooY+KGOUXWm" +
                "ihhT5ZdB0wdznlXye4fLIZdo0E44ZvEq7yluspsp6eHg05DdM/pL2/RixYb6TBqadcQ1WrDWM0KFUPffMG6Sl/C77LQjVai9XvEvM1wUXlL3EcLip3+Wne547PHI2x1qcj7P418seja3zB5AUTu" +
                "Gz9CVDmGHP/ZNr0t4H+T0DHR4BzPd/0Q0Ss95vq4gZ0gB9GIPl60/VbEMNqPQ47ygogF6Nr8g8A85XyAdR+bo3OYPaPesz1UoMOjIX2yJCiQ7G1A4ks4I/Niy0oCBhGGJCsnPgZnmirZPSMCz3s" +
                "bgA4+kRK0UmJ8nQNRKEOZU0jW2TQY1+Vs2/lAQA9q7Lpi30wLvVsw19MtKNk2oGKjGmr2Z7HA6/oEB2m80FJ8v1nnODxBFnaP1hFJvIlUib8liJSxrQNms+1ESnIfCMRYE3uqVa5INeo6ZjTnEc" +
                "dFJaalb8vtsz/thgJSyooN+mPV8s4FfuOFbBN92tZ5CNTVDX9xgG5nqhan4i/ZPkrKG6g25OhntRRBnbL25bgV0BPZLlVCvAm5Y1SgXHREd2Uk1TGNGWkcwnJS10uc9rgeLmoRNjoQLu3DjK2U4" +
                "5T9A2PEgG3nXYpe+mPbVs3F3l0Jp5E74J5o/QLES/jNM4NMI52BYw+ogtQ7nvfwxjvOgXbs454HN2jnXuWNrnc0cNlmXMZPd2u85NxfpP9mLv/EfrucDf7LfYNQoxnDCn68vvp9HHf3T4bpsfop" +
                "h3P7uQDxg1p8vm+u570d7t3OD+PaXMLaO8mwbFedUE3eYH6NN/Y0gefjp2eXMe9NhY2QC1wF/S7e7vg2fHYYyW6MTnGqs/29bhLN10mL0D3ONvS+PezRH4B7AsttNCHRcwdCy200EKfDLoKmAuS" +
                "9ZS7G7vfLW8Go6RBKMjWMPP9cr04l0mDUVDuck0NTEB1ALyedozNXn4zNDXuRHFtqadecBGehqudk4/xijlcDEn7Yt76q+4elvndfofW20iLLNChTY9pPfWPu102dzveYHdjlQv5TjHD3ahN6JN" +
                "N1+Jz28Qz/pC8u6xvAbhigOr9Dig/o2yzAnSDmTfo2w7TIEi3Hf6LSgDF5aNodCOA2gQhabqr5Q+PXpQF6uId+IJpD1UQ3vk6uocabzcpQSy9ph41sF7NByCnvCASGxfwa++nHXXDRQ6Z6iK0k7" +
                "SzMs1/5G3X7gZR6XMnpV5/DtsgF92mjaJbzQL3lCmPmO3yL/VyyOlf6WuRqboo8nhJFhn+11jslfopKyJBXx6LSXumWJMNt/2t/QlZliZJlX/mw6/cZJO/imi3CJ1TxJueQ9TkKb8gYscLw07a9" +
                "T21tHFJ50dKjjzVcqjRbeXIlXYExqlEXvWRM2t5rm448sSq+3IOKgA26fDEsBWEW2aLqWNZgLWyqUNdNRYZLmHlkIiMeqmhMusqnxA9bdl6tiQo77pHztvAld9Vl8eksHqsY31+fHilzD5wMUeG" +
                "CyuAnQu2tEdP6Ib1FPd2SZvcq+m/NxpC3HaCa+u7KbyXJnIAdj+PsPIm3dmmqr3kfdsjQFygTXo33aaRB91pd3vYjr67fsyo0L7D7tL5fLaNI9iCuIcnw+RO8QL03E6cKMcyv+n+iLo79Bx3D6f" +
                "Xu/vTl9DvsnXGtyyFF2g7FgKQBfEZYx0njRn7JlciNeI4A6wlQbYPJW1H2X+BrK8StU+/tbKbUFsm7+qk9onMtlzeBwatzcvUlshLbVM6v9ohzz4cmi+uoWV5/EILLfSBkGPlQgsttNDHlq7zlm" +
                "uM/R5BAC1An5MG5LcJAvND3y3XYNNw9PvkAmm9KMpKeogwMHt12oZ5LVYNQA02TVgBPKA8n0hrBik6euX1HEFa6YQ1IP1V5G/1xxwKIBpjMl5iLPmx35pajf3xHcCzO69juz/CNs/u6UCJI1rbx" +
                "uzeAM5X/QkA1HfJrat9rv9dgL7C8t91q34FKB8JK3TuAOViCN8Z32LmbwiiAVjw4wAUx2iNx/MNQFjBAEkAsIlTFsCbbssyT1P+JgGjgm4QA6DXesjIR68gPqjBmEMVFCMRXdTb8wtgl1dAe3gG" +
                "0QialVN/66O6bUeADhqiXHk1Wyf9IvawynEVWeOUewhiupaXUeXSjVJ2Xs5f87Tv70XBOc9jLe0qW489MkFhZuQX8ZouRA/4R8LOpy5FrdDzUZOhYLgZb6ZnT5F4TqLFVGt16ehcTU7Eedk+OYv" +
                "tc3qXH0FwWOQi4p/oSLqcBfJpk4s/Z4t0zRNIJEZG8Jp6nIsKevlFz2UDZiMvji3tlqXvPqUqZeXiNq4gv9ajm1EucC918xU7cgH8AmdvkSKrHtsnoiztRxbezjKbQj+8AtpN227Rbd/UWcroAL" +
                "pKP7mQS5upJ+j2AQCyu9FlJ6lrP/L+OcG2PAfFu14+9eZt7hOGFbeN78JwgXJWx8k3YIrsMK0JbjrnJnRHnCvB+i0aOyUMgHX1+Nv1Q26ZyacBPirwvfWjbVkK72fcysnx+HvGLr+//ngCrHdPu" +
                "ifod6/44kV3zDUt6CUfb7vkA8+0A8+TeN275d8iuHLpl5B1vHXsdnz+C4LkuHvV6qerqC2NF7g7Pjv2H9rU8zK5EZ1tNS+73n5B+/PS4mlfaKGFnpcYLxdaaKGFPn50HTDX4HIZu8C8eUsaCbI1" +
                "rDQKD71rqKFmfXdj19jSYz4H8BqI36BtvS8NmGtcNg8MOnt1aJDq3dEohbKMXW+N5cgjcxfL3Q3fNjFIBeVqE0M8wo6/pW/4NuAZi3qHnT64s5M4BGsYsxkzHEDebTCxNzkPmOp6kSkbAe160zG" +
                "pgalZto4FvnWbdjAAxj91NeuRVwQ8HYXqFYxOblYtKkB1gC/QQDDsm+ApJwWo5RgAx+qoQNm8LaYrHmKx7QtITh1PkQA2eaQsB0ekjEbIqzu2evWg2zL1adlVvXSz6FXG/qe/KCs6qO9hFScdsg" +
                "HYtmGXlPU8pS3+hW/d0h/bST7Av9Qp5dYxDSqyXuToQ0lXmaJT/bYPRdcsb91C9oniUmCsrgsyaBK51QqzspIqdSS7AlWBSpfbC10UeZpq+V5sXpnjKBG059fDMk8iJ7YJJsV/T8h5vsa1y6Y5q" +
                "UkrTH3TNezrJrSyuUxNe5pIK8svkPa53L1QgvNyGm1HIK28IcCW+oLcKh+gXNIBzaUeWo3lebekDAY3B+kC9AOuEU0dFACws4NbgHbjK2sZ8rv0UZAunqVsTdoyNVE3aXqFbPrALWD/fG8FHnXC" +
                "t2fI6G3v8iG1LKFPv2jb46AN++OLKfTZ+h5EHgSA/AHZ1BzXu3LOLFrV2z1eeBiMX37ODdC+jYdef/GGAcixzEeBPhjY0OPHIHjfX1+d5W0WDtgnfkPGswLYx+4EgH4GUF8DZR93p5Ov8jAOTgJ" +
                "bx19vLneEd2zcEhNlzDwlfR1Idkx2HPfh5y8gqxdcefPFs3++f4hj/qGx/ipyAGle/baTfPO2X0e253HZnqEtkX9eWgD7Qgst9CxiXlhooYUW+nhQXR54cFwSFLv80Q3fflXGjDTONJS+Q7jKWy" +
                "6QN2iI+Q76ITl09H+VGKOyV6eem0YalS531wB1+aOGotay/XqZWI+7xugKYH7SvdKvMfd22NzFw65TbgdYH/vNXSzwI630YId4yt3sbQMQX7vHc3cL63sl4vQ8oBO7WwyBze5mcJv14GfT+qPN1" +
                "J+6bF1MgR1+Jsp1kzSxOODVurHrPRxA/aRPXQApuBYSB6iKG8Q44HEAMSZ3gHApq7KgF+QKKEaerjSA7CMG23bj6ID32q56Yetp9zCKnP/UHdkqZ9/2AJ+0ZZFNUJ9lJQ+fNu0PUjVv/zmzlieN3" +
                "oJgWv/DT9jL13xAd3iRFRV53gKmS/k+tj1+qvJ7yLFMrscYmVI/ySpvqhsAO+II//Dfk5kN94MD+OOTF/vTrft+Q6L1l7Lesig0L1l3la0CCtmH5yEV0beJa+ipVcNn3XSKvr3Ks2k6MUOdqZZZO" +
                "J1NXPocIeWnXEsj98AWHBhvcDkdnEBC7banpJ6sls5pUpVcyZOlfIlTVso5TwHe8otMgsBW7cbwuV28KvQemxfwRg2cvUxClqvnVyz8AGI0lbRPvfLT6Q33hlMJvKRtA9kK2umpbVjXTdkBzTM9K" +
                "VcH5ch5DOiJhx3lOy74yFY5ZIKFXfCOnG+SwItHPlebC2HOAX1gvudl5yZ4kc8tEp1W8BgA7QBtgbCg3Gd+464fV/5IIPGjfoeu3NYVrLvMvR8fMjLpUT+m/AkHepv0Wa54R5Oea8YjHaYzAPvpI" +
                "7+3vtKDz0FyJJ7tHKSnT7j+mPS7k55qLrQA85xUwmPyjp+CXEG+D1E92J8jOJ4eAshtmbs7tH8fud+WWUn59g667QmgXclkr3zHXXpeb7sPbn3lqXnby8PV68m5xOXxDbjbZ1cR3JCWZfELLbTQU" +
                "+Q4ttBCCy30U6XrvOUaSv82QU/H5U2A9GD8S4KG3lOoAxI8/xLBehqIgniNp8vkO4+C7M8RNPpEVY00+ATfAm11iJCw8mOZCuC1vt1kaYc5t+teCDD3XXDtZm3rgPI1dvExtW7pFsbS1WmG+ev75" +
                "DvM5jWgHBO7P8Uc3iDv58+wx8su6fyvy9YHP06OPoHcYB8B5atVALnYjza138nwD5keQA7ojvNLAKzjLx5wl6tTSeBdwbOhglpjClzSLbZSH4csEsihC6KzxJ0y8mlHYFw84L2nz/4FNNun4ilPm" +
                "ro+ABCn0k/iyhf006rIwdMTnelDad96JRat7PsJr5V7HJEpPA8sekh7jNGX893qVN0J1i9xkzdvupQR9Coe01lB9Kqk+zUdWXOwZOH5GXlkAdpHVPDAnmXT/0wR18oOgLYDhW65H3ZbsDy8sQL55" +
                "AH6ZwL67US6AnxPOtX3oeVrLOUyzM2Rnym3YCn3F01aIB3kLT86jatcjQHFrv8WcF4sa+my87vAGp7y6PeGET8GlCMU8O1NhkwwZ+TUXfoQwF3y6A3A5/ZGNm0KyEt7tFXjPFBAl20FsNc2UMQBx" +
                "uPO9eWDBWXsOzzq+C9PkzjnQufIuDg9B5DjhDUOjDTuGF/006ERGcA6zOkYURrxhsq771v0HhO7dsed4X1fndFr8nvs8jzgW4PrfFCmZ/0MwH5Gp7bl9qrvyHtDAspXjJl+hf0JTZxOj8g5Fuu5d" +
    }
}