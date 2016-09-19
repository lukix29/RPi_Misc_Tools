<!DOCTYPE html>
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
    $ssid = $_GET["ssid"];
    $key = $_GET["pw"];
    if(strlen($key) > 0)
    {
        echo "<h1>SSID: " . $ssid . "</h1>";
        echo "<p>Key: " . $key . "</p>";
        $network = "network={
        \r\n\tssid=\"".$ssid ."\"\r\n\tpsk=\"" . $key . "\"\r\n\tkey_mgmt=WPA-PSK
        \r\n}";
        $file = "/etc/wpa_supplicant/wpa_supplicant.conf";
        echo "<p>" . $network . "</p>";
        file_put_contents($file, $network, FILE_APPEND | LOCK_EX);
		echo "<h2>Cannot open file: " . $file . "</h2>";
    }
    else
    {
        echo "<h1>No Key!</h2>";
    }
    echo "<form action=\"wlans.php\">
        <input type=\"submit\" value=\"Back\" />
        </form>";
    ?>
</body>
</html>