struct Cycle
{
	pub lines: Vec<String>,
	pub parameters: Vec<Parameter>,
	pub output: Vec<String>
}

struct Parameter
{
	pub String typeOfParameter,
	pub String contents
}
		
impl Cycle
{
	pub fn readFromFile(&self, fileName: String)
	{
		let mut file = File::open(fileName);
		let mut contents = String::new();
		file.read_to_string(&mut file_contents)
        .ok();
		lines: Vec<String> = file_contents.split("\n")
        .map(|s: &str| s.to_string())
        .collect();
	}
	
	pub fn parse(&self)
	{
		output = parameters.map(|p: Parameter| matchFromParameter(p)); 
	}
		
	pub fn matchFromParameter(&self, param: Parameter)
	{
		
	}
}
