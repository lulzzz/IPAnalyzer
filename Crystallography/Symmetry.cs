using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Crystallography
{
    /// <summary>
    /// Symmetry ÌTvÌà¾Å·B
    /// </summary>
    [Serializable()]
    public class Symmetry
    {
        //sub,SF,Hall,HM,HM_full,1²p,1²v,2²p,2²v,3²p,3²v,_QAEGQA»n
        public string SpaceGroupHMsubStr;

        public string SpaceGroupSFStr;
        public string SpaceGroupHallStr;
        public string SpaceGroupHMStr;
        public string SpaceGroupHMfullStr;
        public string MainAxis;
        public string LatticeTypeStr;

        public string StrSE1p;
        public string StrSE1v;
        public string StrSE2p;
        public string StrSE2v;
        public string StrSE3p;
        public string StrSE3v;
        public string PointGroupHMStr;
        public string PointGroupSFStr;
        public string LaueGroupStr;
        public string CrystalSystemStr;

        //Unknown;530óÔQÌÔ(ÊµÔ		óÔQÔ	óÔQÌSubÔ		_QÔ	EGQÔ	»nÔ)
        public int SeriesNumber, SpaceGroupNumber, SpaceGroupSubNumber, PointGroupNumber, LaueGroupNumber, CrystalSystemNumber;

        [XmlIgnoreAttribute]
        public string[] ExtinctionRuleStr;

        public bool IsPlaneRootIndex(int h, int k, int l) => SymmetryStatic.IsRootIndex(h, k, l, this);

        public List<Func<int, int, int, string>> CheckExtinctionFunc { get; set; } = new List<Func<int, int, int, string>>();

        public string[] CheckExtinctionRule(int h, int k, int l) 
            => CheckExtinctionFunc.Select(check => check(h, k, l)).Where(str => str != null).ToArray();

        public enum CrystalSytem { Unknown, Triclinic, Monoclinic, Orthorhombic, Tetragonal, Trigonal, Hexagonal, Cubic }

        public enum LatticeType { P, A, B, C, I, F, R }
    }
}