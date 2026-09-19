using System;
using GoogleMapPlugin.Models;

namespace GoogleMapPlugin.Services
{
    public class CoordinateService
    {
        // =====================================================
        // WGS84
        // =====================================================

        private const double A = 6378137.0;

        private const double INV_F =
            298.257223563;


        // =====================================================
        // 7 THAM SỐ TOWGS84
        // LẤY ĐÚNG TỪ PYTHON CỦA BẠN
        // =====================================================

        private const double DX =
            -191.90441429;

        private const double DY =
            -39.30318279;

        private const double DZ =
            -111.45032835;

        private const double RX =
            -0.00928836;

        private const double RY =
             0.01975479;

        private const double RZ =
            -0.00427372;

        private const double DS =
             0.252906278;


        // =====================================================
        // VN2000 → WGS84
        // =====================================================

        public Wgs84Coordinate ToWgs84(
            double x,
            double y,
            double ktt)
        {
            // -------------------------------------------------
            // 1. Ellipsoid
            // -------------------------------------------------

            double f =
                1.0 / INV_F;

            double e2 =
                f * (2.0 - f);


            // -------------------------------------------------
            // 2. VN2000 → tọa độ địa lý
            // -------------------------------------------------

            double[] geo =
                InverseTM(
                    x,
                    y,
                    ktt,
                    A,
                    e2,
                    0.9999);


            double lat =
                geo[0];

            double lon =
                geo[1];


            // -------------------------------------------------
            // 3. Geodetic → ECEF
            // -------------------------------------------------

            double[] xyz =
                GeodeticToECEF(
                    lat,
                    lon,
                    0.0,
                    A,
                    e2);


            double X =
                xyz[0];

            double Y =
                xyz[1];

            double Z =
                xyz[2];


            // -------------------------------------------------
            // 4. Helmert 7 tham số
            // -------------------------------------------------

            double[] wgs =
                HelmertTransform(
                    X,
                    Y,
                    Z);


            // -------------------------------------------------
            // 5. ECEF → WGS84 Geodetic
            // -------------------------------------------------

            double[] result =
                ECEFToGeodetic(
                    wgs[0],
                    wgs[1],
                    wgs[2],
                    A,
                    e2);


            double latitude =
                RadiansToDegrees(
                    result[0]);

            double longitude =
                RadiansToDegrees(
                    result[1]);


            return new Wgs84Coordinate(
                longitude,
                latitude);
        }


        // =====================================================
        // INVERSE TRANSVERSE MERCATOR
        // =====================================================

        private double[] InverseTM(
            double x,
            double y,
            double lon0Degree,
            double a,
            double e2,
            double k0)
        {
            double x0 =
                500000.0;

            double y0 =
                0.0;


            double xx =
                x - x0;

            double yy =
                y - y0;


            double M =
                yy / k0;


            double mu =
                M /
                (
                    a *
                    (
                        1.0
                        - e2 / 4.0
                        - 3.0 * e2 * e2 / 64.0
                        - 5.0 * e2 * e2 * e2 / 256.0
                    )
                );


            double e1 =
                (
                    1.0 -
                    Math.Sqrt(1.0 - e2)
                )
                /
                (
                    1.0 +
                    Math.Sqrt(1.0 - e2)
                );


            double e1_2 =
                e1 * e1;

            double e1_3 =
                e1_2 * e1;

            double e1_4 =
                e1_3 * e1;


            double phi1 =
                mu

                + (
                    3.0 * e1 / 2.0
                    - 27.0 * e1_3 / 32.0
                  )
                  * Math.Sin(2.0 * mu)

                + (
                    21.0 * e1_2 / 16.0
                    - 55.0 * e1_4 / 32.0
                  )
                  * Math.Sin(4.0 * mu)

                + (
                    151.0 * e1_3 / 96.0
                  )
                  * Math.Sin(6.0 * mu)

                + (
                    1097.0 * e1_4 / 512.0
                  )
                  * Math.Sin(8.0 * mu);


            double sinPhi =
                Math.Sin(phi1);

            double cosPhi =
                Math.Cos(phi1);

            double tanPhi =
                Math.Tan(phi1);


            double N =
                a /
                Math.Sqrt(
                    1.0 -
                    e2 *
                    sinPhi *
                    sinPhi);


            double R =
                a *
                (1.0 - e2)
                /
                Math.Pow(
                    1.0 -
                    e2 *
                    sinPhi *
                    sinPhi,
                    1.5);


            double T =
                tanPhi * tanPhi;


            double C =
                e2 /
                (1.0 - e2)
                *
                cosPhi *
                cosPhi;


            double D =
                xx /
                (N * k0);


            // -------------------------------------------------
            // Latitude
            // -------------------------------------------------

            double lat =
                phi1

                - (
                    N *
                    tanPhi /
                    R
                  )
                  *
                  (
                      D * D / 2.0

                      - (
                          5.0
                          + 3.0 * T
                          + 10.0 * C
                          - 4.0 * C * C
                          - 9.0 *
                            e2 /
                            (1.0 - e2)
                        )
                        *
                        Math.Pow(D, 4)
                        / 24.0

                      + (
                          61.0
                          + 90.0 * T
                          + 298.0 * C
                          + 45.0 * T * T
                          - 252.0 *
                            e2 /
                            (1.0 - e2)
                          - 3.0 * C * C
                        )
                        *
                        Math.Pow(D, 6)
                        / 720.0
                  );


            // -------------------------------------------------
            // Longitude
            // -------------------------------------------------

            double lon0 =
                DegreesToRadians(
                    lon0Degree);


            double lon =
                lon0

                + (
                    D

                    - (
                        1.0
                        + 2.0 * T
                        + C
                      )
                      *
                      Math.Pow(D, 3)
                      / 6.0

                    + (
                        5.0
                        - 2.0 * C
                        + 28.0 * T
                        - 3.0 * C * C
                        + 8.0 *
                          e2 /
                          (1.0 - e2)
                        + 24.0 * T * T
                      )
                      *
                      Math.Pow(D, 5)
                      / 120.0
                  )
                  /
                  cosPhi;


            return new double[]
            {
                lat,
                lon
            };
        }


        // =====================================================
        // GEODETIC → ECEF
        // =====================================================

        private double[] GeodeticToECEF(
            double lat,
            double lon,
            double h,
            double a,
            double e2)
        {
            double sinLat =
                Math.Sin(lat);

            double cosLat =
                Math.Cos(lat);

            double sinLon =
                Math.Sin(lon);

            double cosLon =
                Math.Cos(lon);


            double N =
                a /
                Math.Sqrt(
                    1.0 -
                    e2 *
                    sinLat *
                    sinLat);


            double X =
                (N + h)
                * cosLat
                * cosLon;


            double Y =
                (N + h)
                * cosLat
                * sinLon;


            double Z =
                (
                    N *
                    (1.0 - e2)
                    + h
                )
                * sinLat;


            return new double[]
            {
                X,
                Y,
                Z
            };
        }


        // =====================================================
        // HELMERT 7 PARAMETERS
        // =====================================================

        private double[] HelmertTransform(
            double X,
            double Y,
            double Z)
        {
            // -------------------------------------------------
            // Arc-second → radian
            // -------------------------------------------------

            double secToRad =
                Math.PI /
                (
                    180.0 *
                    3600.0
                );


            double rx =
                RX *
                secToRad;

            double ry =
                RY *
                secToRad;

            double rz =
                RZ *
                secToRad;


            // -------------------------------------------------
            // ppm → scale
            // -------------------------------------------------

            double scale =
                1.0 +
                DS * 1.0e-6;


            // -------------------------------------------------
            // Position Vector convention
            // -------------------------------------------------

            double X2 =
                DX +
                scale *
                (
                    X
                    - rz * Y
                    + ry * Z
                );


            double Y2 =
                DY +
                scale *
                (
                    rz * X
                    + Y
                    - rx * Z
                );


            double Z2 =
                DZ +
                scale *
                (
                    -ry * X
                    + rx * Y
                    + Z
                );


            return new double[]
            {
                X2,
                Y2,
                Z2
            };
        }


        // =====================================================
        // ECEF → GEODETIC
        // =====================================================

        private double[] ECEFToGeodetic(
            double X,
            double Y,
            double Z,
            double a,
            double e2)
        {
            double lon =
                Math.Atan2(
                    Y,
                    X);


            double p =
                Math.Sqrt(
                    X * X +
                    Y * Y);


            double lat =
                Math.Atan2(
                    Z,
                    p * (1.0 - e2));


            // -------------------------------------------------
            // Lặp để tìm latitude
            // -------------------------------------------------

            for (int i = 0; i < 10; i++)
            {
                double sinLat =
                    Math.Sin(lat);


                double N =
                    a /
                    Math.Sqrt(
                        1.0 -
                        e2 *
                        sinLat *
                        sinLat);


                lat =
                    Math.Atan2(
                        Z + e2 * N * sinLat,
                        p);
            }


            return new double[]
            {
                lat,
                lon
            };
        }


        // =====================================================
        // DEG → RAD
        // =====================================================

        private double DegreesToRadians(
            double degree)
        {
            return
                degree *
                Math.PI /
                180.0;
        }


        // =====================================================
        // RAD → DEG
        // =====================================================

        private double RadiansToDegrees(
            double radian)
        {
            return
                radian *
                180.0 /
                Math.PI;
        }
    }
}