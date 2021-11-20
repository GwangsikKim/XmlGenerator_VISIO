import os

def write_csv(file_path, dataset):
    with open(file_path, "w") as f:
        for data in dataset:
            string = str(data[0])+", " +str(data[1])+", "+ str(data[2]) +", "+ str(data[3]) +'\n'
            f.write(string)
