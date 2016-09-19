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
    exec("iwlist wlan0 scan", $output);
	
    echo "<h1>Available WLAN Networks:</h1>";
    $cnt = 1;
    foreach($output as $si)
    {
        if (strpos($si, "ESSID") !== false)
        {
            $repl = array("ESSID:","\""," ");
            $ssid = str_replace($repl, "", $si);

            echo "<p>--------" . $cnt++ . "--------</p>";
            echo "<form action=\"addwpa.php\">
        <p>SSID:<input type=\"text\" name=\"ssid\" value=\"" . $ssid . "\"/>
        <p>Password:<input type=\"text\" name=\"pw\"/>
        <input type=\"submit\" value=\"Add\"></p>
        </form>";
        }
    }
    ?>

    <form action="wlans.php">
        <input type="submit" value="Reload" />
        <br>
        <br />
    </form>
    <form action="index.php">
        <input type="submit" value="Back" />
    </form>

</body>
</html>
