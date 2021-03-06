﻿using System.Windows;

namespace Improbable.Projections
{
    public interface IMapProjection
    {
        Point ToSphere(Point point);

        Point ToSphere(double x, double y);

        Point ToPlane(Point point);

        Point ToPlane(double lat, double lon);
    }
}
