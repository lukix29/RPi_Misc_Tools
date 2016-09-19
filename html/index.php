<!DOCTYPE html>
<meta http-equiv="refresh" content="10" />
<style>
    body {
        background-color: black;
    }

    h1 {
        color: white;
    }

    h2 {
        color: white;
    }

    p {
        color: white;
    }
</style>
<html>
<body>

    <?php
	if(file_exists("/sys/devices/w1_bus_master1/28-0000067d7513/w1_slave")==TRUE)
	{
		echo "<h1>Temperatures</h1>";
	}
	else
	{
		echo "<h1>Temperature</h1>";
	}
    externTemp();
    cpuTemp();
    function externTemp()
    {
		if(file_exists("/sys/devices/w1_bus_master1/28-0000067d7513/w1_slave")==TRUE)
		{
			$s = file_get_contents("/sys/devices/w1_bus_master1/28-0000067d7513/w1_slave");
			$sa = split("t=", $s, 2);
			$val = intval($sa[1]);
			$si = $val / 1000.0;
			echo "<h2>Extern:\t" . $si . " *C</h2>";
		}
	}
	function cpuTemp()
    {
        $s = file_get_contents("/sys/class/thermal/thermal_zone0/temp");
        $val = intval($s);
        $si = $val / 1000.0;
        echo "<p><img src=\"images/cpu.png\" alt=\"CPU\" style=\"float:left;width:42px;height:42px;\">" . $si . " *C</p>";
    }
    echo "<br>";
    $b = exec("ls /dev/video*");
	$query = "/dev/video";
	if (substr($b, 0, strlen($query)) === $query) 
	{
		echo "<a href=\"camera.php\" target=\"_self\">";
		echo "<input type=\"Submit\" value=\"Camera\" /></a>\r\n";
	}
    $a = exec("ifconfig -a");
	if (strpos($a, 'wlan') !== false) 
	{
		echo "<a href=\"wlans.php\" target=\"_self\">";
		echo "<input type=\"Submit\" value=\"Scan WiFi Networks\" /></a>\r\n";
	}	
    echo "<a href=\"hwrng.php\">";
	echo "<input type=\"Submit\" value=\"HWRNG\" /></a>\r\n";	

	echo "<a href=\"files/Bubble_UPNP.apk\">";
	echo "<input type=\"Submit\" value=\"Bubble_UPNP APK\" /></a>";	
	?>

</body>
</html>
