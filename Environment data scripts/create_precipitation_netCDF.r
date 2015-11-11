# Load required libraries
library(rgdal)
library(RNetCDF)

# Create the netCDF to hold all of the data
nc<-create.nc("G:/WCMC Work/Environmental Data/Climate/monthly/1_degree/Precipitation.nc")

# Create vectors of data corresponding to latitude, longitude and month dimensions
lats<-(89.5:-89.5)
lons<-(-179.5:179.5)
months<-1:12

# Define the dimensions in the netCDF
dim.def.nc(nc,"Lat",180)
dim.def.nc(nc,"Lon",360)
dim.def.nc(nc,"Month",12)

# Define the variables in the netCDF
var.def.nc(nc,"Lat","NC_DOUBLE","Lat")
var.def.nc(nc,"Lon","NC_DOUBLE","Lon")
var.def.nc(nc,"Month","NC_INT","Month")
var.def.nc(nc,"Precipitation","NC_DOUBLE",c("Lat","Lon","Month"))

# Add the missing-data attribute to the temperature variable in the netCDF
att.put.nc(nc,"Precipitation","missing_value","NC_DOUBLE",-9999.0)

# Add the dimension data to the corresponding netCDF variables
var.put.nc(nc,"Lat",lats,1,length(lats))
var.put.nc(nc,"Lon",lons,1,length(lons))
var.put.nc(nc,"Month",months,1,length(months))

# Loop over months
for (f in 1:12)
{
  # Construct the filepath for this month's temperature data
  fname<-paste("G:/WCMC Work/Environmental Data/Climate/monthly/1_degree/prec/prec_",f,sep="")
  # Read the file
  map<-readGDAL(fname)

  # Replace NAs with the missing data value
  map$band1[is.na(map$band1)]<-(-9999.0)

  # Make an array to hold the data
  monthData<-array(-9999.0,c(180,360))
  # Fill the array with values from the map for this month
  monthData[1:150,]<-matrix(map$band1,ncol=360,byrow=T)

  # Write the data to the output netCDF
  var.put.nc(nc,"Precipitation",monthData,c(1,1,f),c(length(lats),length(lons),1))
}

# Sync and close the netCDF
sync.nc(nc)
close.nc(nc)
