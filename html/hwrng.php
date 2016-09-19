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

$value;// = file_get_contents("rngstring"); 
exec("mono ./files/rng.exe", $value);
foreach($value as $string)
{
    echo "<p>".$string."</p>";
}
echo "<meta http-equiv=\"refresh\" content=\"10\" />";

echo "<br>";
echo "<a href=\"index.php\" target=\"_self\">";
echo "<input type=\"Submit\" value=\"Back\" /></a>\r\n";

echo "<a href=\"hwrng.php\" target=\"_self\">";
echo "<input type=\"Submit\" value=\"Reload\" /></a>";
?>

</body>
</html>

