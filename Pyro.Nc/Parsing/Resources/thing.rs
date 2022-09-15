fn main()
{	
println!("joki");
let mut buffer: Vec<u8> = vec![];
fs:read_file("cache.ch", buffer);
let str: &str = String::from(buffer);
let mut arr: Vec<&str> = splitCode(str);
}

fn splitCode(code: &str) -> Vec<&str>
{
let mut arr: Vec<&str> = Vec::new();
let mut part: &str = "";
for char in code
{
if char == ' '
{arr.push(part);}
else{part+=char;part.clear();}
}
}