﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FNPlugin {
    class VanAllen {
        const double B0 = 3.12E-5;
		public static Dictionary<ProtoCrewMember,double> crew_rad_exposure = new Dictionary<ProtoCrewMember, double> ();
                
        public static float getBeltAntiparticles(int refBody, float altitude, float lat) {
            lat = (float) (lat/180*Math.PI);
            CelestialBody crefbody = FlightGlobals.fetch.bodies[refBody];
            CelestialBody crefkerbin = FlightGlobals.fetch.bodies[1];

			double atmosphere_height = PluginHelper.getMaxAtmosphericAltitude (crefbody);
            if (altitude <= atmosphere_height && crefbody.flightGlobalsIndex != 0) {
                return 0;
            }

            double mp = crefbody.Mass;
            double rp = crefbody.Radius;
            double rt = crefbody.rotationPeriod;
            double relmp = mp / crefkerbin.Mass;
            double relrp = rp / crefkerbin.Radius;
            double relrt = rt / crefkerbin.rotationPeriod;

            double peakbelt = 1.5 * crefkerbin.Radius * relrp;
            double altituded = ((double)altitude);
            double a = peakbelt / Math.Sqrt(2);
            double beltparticles = Math.Sqrt(2 / Math.PI)*Math.Pow(altituded,2)*Math.Exp(-Math.Pow(altituded,2)/(2.0*Math.Pow(a,2)))/(Math.Pow(a,3));
            beltparticles = beltparticles * relmp * relrp / relrt*50.0;

            if (crefbody.flightGlobalsIndex == 0) {
                beltparticles = beltparticles / 1000;
            }

			beltparticles = beltparticles * Math.Abs(Math.Cos(lat)) * getSpecialMagneticFieldScaling(refBody);

            return (float) beltparticles;
        }

        public static float getBeltMagneticFieldMag(int refBody, float altitude, float lat) {
            lat = (float)(lat / 180 * Math.PI);
            CelestialBody crefbody = FlightGlobals.fetch.bodies[refBody];
            CelestialBody crefkerbin = FlightGlobals.fetch.bodies[1];

            double mp = crefbody.Mass;
            double rp = crefbody.Radius;
            double rt = crefbody.rotationPeriod;
            double relmp = mp / crefkerbin.Mass;
            double relrp = rp / crefkerbin.Radius;
            double relrt = rt / crefkerbin.rotationPeriod;



            double altituded = ((double)altitude)+rp;
            double Bmag = B0 / relrt *relmp * Math.Pow((rp / altituded), 3) * Math.Sqrt(1 + 3 * Math.Pow(Math.Cos(lat), 2)) * getSpecialMagneticFieldScaling(refBody);

            return (float)Bmag;
        }

        public static float getBeltMagneticFieldRadial(int refBody, float altitude, float lat) {
            lat = (float)(lat / 180 * Math.PI);
            CelestialBody crefbody = FlightGlobals.fetch.bodies[refBody];
            CelestialBody crefkerbin = FlightGlobals.fetch.bodies[1];
                        
            double mp = crefbody.Mass;
            double rp = crefbody.Radius;
            double rt = crefbody.rotationPeriod;
            double relmp = mp / crefkerbin.Mass;
            double relrp = rp / crefkerbin.Radius;
            double relrt = rt / crefkerbin.rotationPeriod;

            double altituded = ((double)altitude) + rp;
			double Bmag = -2 / relrt *relmp * B0 * Math.Pow((rp / altituded), 3) * Math.Cos(lat)* getSpecialMagneticFieldScaling(refBody);

            return (float)Bmag;
        }

        public static float getBeltMagneticFieldAzimuthal(int refBody, float altitude, float lat) {
            lat = (float)(lat / 180 * Math.PI);
            CelestialBody crefbody = FlightGlobals.fetch.bodies[refBody];
            CelestialBody crefkerbin = FlightGlobals.fetch.bodies[1];

            double mp = crefbody.Mass;
            double rp = crefbody.Radius;
            double rt = crefbody.rotationPeriod;
            double relmp = mp / crefkerbin.Mass;
            double relrp = rp / crefkerbin.Radius;
            double relrt = rt / crefkerbin.rotationPeriod;

            double altituded = ((double)altitude) + rp;
			double Bmag = -relmp * B0 / relrt * Math.Pow((rp / altituded), 3) * Math.Sin(lat)* getSpecialMagneticFieldScaling(refBody);

            return (float)Bmag;
        }

		public static double getSpecialMagneticFieldScaling(int refBody) {
			double special_scaling = 1;
			if (refBody == PluginHelper.REF_BODY_TYLO) {
				special_scaling = 7;
			}
			if (refBody == PluginHelper.REF_BODY_LAYTHE) {
				special_scaling = 5;
			}
			if (refBody == PluginHelper.REF_BODY_MOHO) {
				special_scaling = 2;
			}
			if (refBody == PluginHelper.REF_BODY_JOOL) {
				special_scaling = 3;
			}
			if (refBody == PluginHelper.REF_BODY_EVE) {
				special_scaling = 2;
			}
			if (refBody == PluginHelper.REF_BODY_MUN || refBody == PluginHelper.REF_BODY_IKE) {
				special_scaling = 0.2;
			}
			if (refBody == PluginHelper.REF_BODY_GILLY || refBody == PluginHelper.REF_BODY_BOP || refBody == PluginHelper.REF_BODY_POL) {
				special_scaling = 0.05;
			}
			return special_scaling;
		}

        
    }
}
